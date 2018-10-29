using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Migrations.Internal;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore.Extensions;

// ReSharper disable CheckNamespace

namespace Microsoft.EntityFrameworkCore
{
    public class MigratorWithSeeding : IMigrator
    {
        private readonly Migrator _efMigrator;
        private readonly IHistoryRepository _historyRepository;
        private readonly IRelationalDatabaseCreator _databaseCreator;
        private readonly IMigrationsAssembly _migrationsAssembly;
        private readonly IDiagnosticsLogger<DbLoggerCategory.Migrations> _diagnostics;
        private readonly ILogger _logger;
        private readonly ICurrentDbContext _currentDbContext;
        private readonly string _databaseName;

        public MigratorWithSeeding(Migrator efMigrator, IDatabaseCreator databaseCreator, IHistoryRepository historyRepository, IRelationalConnection connection, IDiagnosticsLogger<DbLoggerCategory.Migrations> diagnostics, ICurrentDbContext currentDbContext, IMigrationsAssembly migrationsAssembly)
        {
            _efMigrator = efMigrator ?? throw new ArgumentNullException(nameof(efMigrator));
            _historyRepository = historyRepository ?? throw new ArgumentNullException(nameof(historyRepository));
            _diagnostics = diagnostics ?? throw new ArgumentNullException(nameof(diagnostics));
            _currentDbContext = currentDbContext ?? throw new ArgumentNullException(nameof(currentDbContext));
            _migrationsAssembly = migrationsAssembly ?? throw new ArgumentNullException(nameof(migrationsAssembly));
            _logger = diagnostics.Logger ?? throw new ArgumentNullException(nameof(diagnostics.Logger));
            _databaseCreator = (IRelationalDatabaseCreator) databaseCreator ?? throw new ArgumentNullException(nameof(databaseCreator));
            _databaseName = connection?.DbConnection?.Database ?? throw new ArgumentNullException(nameof(connection));
        }

        public void Migrate(string targetMigration = null)
        {
            AsyncHelper.RunSync(() => MigrateAsync(targetMigration));
        }

        public async Task MigrateAsync(string targetMigration = null, CancellationToken cancellationToken = new CancellationToken())
        {
            var appliedMigrations = (await _historyRepository.GetAppliedMigrationsAsync(cancellationToken)).Select(_ => _.MigrationId).ToList();
            var pendingMigrations = _migrationsAssembly.Migrations.Keys.Except(appliedMigrations).ToList();

            if (string.IsNullOrWhiteSpace(targetMigration))
            {
                await ApplyMigrationsAsync(pendingMigrations, cancellationToken);
                return;
            }

            if (pendingMigrations.Contains(targetMigration))
            {
                await ApplyMigrationAsync(targetMigration, cancellationToken);
                return;
            }

            if (targetMigration == "0" || appliedMigrations.Contains(targetMigration))
            {
                await RevertToMigrationAsync(targetMigration, cancellationToken);
                return;
            }

            _logger.LogTrace("Target migration {Migration} does not exist.", targetMigration);
        }

        public string GenerateScript(string fromMigration = null, string toMigration = null, bool idempotent = false)
        {
            return _efMigrator.GenerateScript(fromMigration, toMigration, idempotent);
        }

        /// <summary>
        /// Applies all pending migrations
        /// </summary>
        /// <param name="pendingMigrations"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        private async Task ApplyMigrationsAsync(IReadOnlyCollection<string> pendingMigrations, CancellationToken cancellationToken = default(CancellationToken))
        {
            var appliedMigrations = await _historyRepository.GetAppliedMigrationsAsync(cancellationToken);
            var currentState = appliedMigrations.Select(x => x.MigrationId).LastOrDefault();

            _logger.LogTrace("There are {MigrationCount} migration(s) to apply to {Database}.", pendingMigrations.Count, _databaseName);

            foreach (var migration in pendingMigrations)
            {
                if (await ApplyMigrationAsync(migration, cancellationToken))
                {
                    continue;
                }

                if (!string.IsNullOrWhiteSpace(currentState))
                {
                    _logger.LogError("Attempting to roll the database {Database} back to the last known working state: {WorkingState}.", _databaseName, currentState);
                    await RevertToMigrationAsync(currentState, cancellationToken);
                }
                else
                {
                    _logger.LogError("Failed to apply the first migration, deleting the superflous database {Database}.", _databaseName);
                    await _databaseCreator.EnsureDeletedAsync(cancellationToken);
                }

                break;
            }
        }

        /// <summary>
        /// <para>Applys the given migration if it is pending, and seeds using any valid seeders available via DI for the migration.</para>
        /// <para>It does not revert the database back to a known working state upon error.</para>
        /// </summary>
        /// <param name="migration"></param>
        /// <param name="cancellationToken"></param>
        /// <returns><c>True</c> if the migration was successfully applied and seeded, <c>False</c> otherwise.</returns>
        public async Task<bool> ApplyMigrationAsync(string migration, CancellationToken cancellationToken = default(CancellationToken))
        {
            try
            {
                var appliedMigrations = (await _historyRepository.GetAppliedMigrationsAsync(cancellationToken)).Select(_ => _.MigrationId);

                if (appliedMigrations.Contains(migration))
                {
                    _logger.LogTrace("Skipping the miration {Migration} as it has already been applied.");
                    return true;
                }

                var seeders = _currentDbContext.Context.GetSeedersForMigration(migration).ToList();

                if (seeders.Any())
                {
                    _logger.LogTrace("Found {SeederCount} seeders for migration {Migration}.", seeders.Count, migration);
                }
                else
                {
                    _logger.LogTrace("No seeders could be found for the migration {Migration}.", migration);
                }

                await SeedAsync(seeders, migration, SeedOperation.Up, SeedHook.Pre, cancellationToken);
                _logger.LogTrace("Attempting to apply the migration {Migration}.", migration);
                await _efMigrator.MigrateAsync(migration, cancellationToken);
                _logger.LogTrace("Successfully applied the migration {Migration}.", migration);
                await SeedAsync(seeders, migration, SeedOperation.Up, SeedHook.Post, cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.HResult, ex, "Was unable to apply the migration {Migration}. See Exception for more details.", migration);
                return false;
            }

            return true;
        }

        /// <summary>
        /// <para>Reverts the database to the given migration, and calls the down methods on any available seeders.</para>
        /// <para>It does not revert the database back to a known working state upon error.</para>
        /// </summary>
        /// <param name="migration"><para>The target migration. Migrations may be identified by name or by ID. The number <c>0</c> is a special case that means before the first migration and causes all migrations to be reverted.</para></param>
        /// <param name="cancellationToken"></param>
        /// <returns><c>True</c> if the database was successfully reverted to the migration, <c>False</c> otherwise.</returns>
        public async Task<bool> RevertToMigrationAsync(string migration, CancellationToken cancellationToken = default(CancellationToken))
        {
            try
            {
                if (migration == "0")
                {
                    _logger.LogTrace("Reverting the database {Database} back to before any migrations have been applied.", _databaseName);
                    await _efMigrator.MigrateAsync(migration, cancellationToken);
                    _logger.LogTrace("Removed all migrations from the database {Database}.", _databaseName);
                    return true;
                }

                var appliedMigrations = (await _historyRepository.GetAppliedMigrationsAsync(cancellationToken)).Select(_ => _.MigrationId).ToList();

                if (!appliedMigrations.Contains(migration))
                {
                    _logger.LogTrace("Cannot remove migration {Migration} as it has not been applied.", migration);
                    return false;
                }

                var migrationIdx = appliedMigrations.IndexOf(migration);
                var migrationsToRemove = new List<string>();

                if (migrationIdx == appliedMigrations.Count)
                {
                    _logger.LogTrace("The migration {Migration} to remove is the last applied migration.", migration);
                    return true;
                }
                
                _logger.LogTrace("The migration {Migration} to remove was not the last applied migration, removing all migrations since the migration was applied.", migration);
                migrationsToRemove.AddRange(appliedMigrations.Skip(migrationIdx + 1).Reverse());
                _logger.LogTrace("The following migrations to remove: {Migrations}.", migrationsToRemove);

                foreach (var migrationToRemove in migrationsToRemove)
                {
                    var seeders = _currentDbContext.Context.GetSeedersForMigration(migrationToRemove).ToList();

                    if (seeders.Any())
                    {
                        _logger.LogTrace("Found {SeederCount} seeders for migration {Migration}.", seeders.Count, migrationToRemove);
                    }
                    else
                    {
                        _logger.LogTrace("No seeders could be found for the migration {Migration}.", migrationToRemove);
                    }

                    await SeedAsync(seeders, migrationToRemove, SeedOperation.Down, SeedHook.Pre, cancellationToken);
                    _logger.LogTrace("Attempting to revert the migration {Migration}.", migrationToRemove);
                    await _efMigrator.MigrateAsync(migrationToRemove, cancellationToken);
                    _logger.LogTrace("Successfully reverted the migration {Migration}.", migrationToRemove);
                    await SeedAsync(seeders, migrationToRemove, SeedOperation.Down, SeedHook.Post, cancellationToken);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.HResult, ex, "Was unable to remove the migration {Migration}. See Exception for more details.", migration);
                return false;
            }

            return true;
        }

        private async Task SeedAsync(IEnumerable<IDbContextSeeder> seeders, string migration, SeedOperation operation, SeedHook hook, CancellationToken cancellationToken = default(CancellationToken))
        {
            _logger.LogTrace("Beginning seeding {Hook} migration ({Migration}) being applied.");

            foreach (var seeder in seeders)
            {
                _logger.LogTrace("Calling the {Hook}{Operation} method on {Seeder} for migration {Migration}.", hook, operation, seeder.GetType().FullName, migration);

                if (operation == SeedOperation.Up)
                {
                    if (hook == SeedHook.Pre)
                    {
                        await seeder.PreUpAsync(cancellationToken);
                    }
                    else
                    {
                        await seeder.PostUpAsync(cancellationToken);
                    }
                }
                else
                {
                    if (hook == SeedHook.Post)
                    {
                        await seeder.PreDownAsync(cancellationToken);
                    }
                    else
                    {
                        await seeder.PostDownAsync(cancellationToken);
                    }
                }
            }

            _logger.LogTrace("Finished seeding {Hook} migration ({Migration}) being applied.", hook, migration);
        }
    }
}