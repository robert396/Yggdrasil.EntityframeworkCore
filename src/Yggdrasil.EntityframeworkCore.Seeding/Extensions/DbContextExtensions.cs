// ReSharper disable CheckNamespace

using Microsoft.EntityFrameworkCore.Infrastructure;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.EntityFrameworkCore.Migrations;
using Yggdrasil.EntityframeworkCore.Seeding.Extensions;

namespace Microsoft.EntityFrameworkCore.Extensions
{
    public static class DbContextExtensions
    {
        internal static IEnumerable<Type> GetSeederTypes<TContext>(this TContext dbContext) where TContext : DbContext
        {
            var migrationsAssembly = dbContext.GetService<IMigrationsAssembly>();

            return migrationsAssembly.Assembly.GetTypes().Where(SeederFilter);
        }

        internal static IEnumerable<Type> GetSeederTypesForMigration<TContext>(this TContext dbContext, string migrationId) where TContext : DbContext
        {
            return (
                from seederType in dbContext.GetSeederTypes()
                let dbContextAttribute = seederType.GetCustomAttribute<DbContextAttribute>()
                    where dbContextAttribute != null && dbContextAttribute.ContextType == dbContext.GetType()
                let migrationAttr = seederType.GetCustomAttribute<SeederForMigrationAttribute>()
                    where migrationAttr != null && migrationAttr.IsValid(migrationId)
                select seederType
            ).ToList();
        }

        internal static IEnumerable<IDbContextSeeder> GetSeedersForMigration<TContext>(this TContext dbContext, string migrationId) where TContext : DbContext
        {
            var types = dbContext.GetSeederTypesForMigration(migrationId);
            return types.Select(type => (IDbContextSeeder) dbContext.GetService(type)).OrderBy(_ => _.GetOrder());
        }

        internal static bool SeederFilter(Type type)
        {
            return type.GetInterfaces().Contains(typeof(IDbContextSeeder)) && !type.IsInterface &&
                   type.GetCustomAttribute<SeederForMigrationAttribute>() != null &&
                   type.GetCustomAttribute<DbContextAttribute>() != null;
        }

        public static DbContextOptionsBuilder UseSeeding(this DbContextOptionsBuilder builder)
        {
            if (builder == null)
            {
                throw new ArgumentNullException(nameof(builder));
            }

            var extension = builder.GetOrCreateExtension(() => new SeederOptionsExtension());
            builder.AddOrUpdateExtension(extension);
            builder.ReplaceService<IMigrator, MigratorWithSeeding>();

            return builder;
        }

        private static TExtension GetOrCreateExtension<TExtension>(this DbContextOptionsBuilder builder, Func<TExtension> extensionFactory) where TExtension : class, IDbContextOptionsExtension
        {
            return builder.Options.FindExtension<TExtension>() ?? extensionFactory();
        }

        private static void AddOrUpdateExtension<TExtension>(this DbContextOptionsBuilder builder, TExtension extension) where TExtension : class, IDbContextOptionsExtension
        {
            if (builder is IDbContextOptionsBuilderInfrastructure optsBuilder)
            {
                optsBuilder.AddOrUpdateExtension(extension);
            }
        }
    }
}