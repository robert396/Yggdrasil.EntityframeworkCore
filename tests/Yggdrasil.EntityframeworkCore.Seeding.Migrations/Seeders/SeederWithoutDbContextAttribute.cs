using Microsoft.EntityFrameworkCore;
using System.Threading;
using System.Threading.Tasks;
using Yggdrasil.EntityframeworkCore.Seeding.Migrations.Migrations;

namespace Yggdrasil.EntityframeworkCore.Seeding.Migrations.Seeders
{
    [SeederForMigration(nameof(CreateUserSchema))]
    public class SeederWithoutDbContextAttribute : IDbContextSeeder
    {
        public Task PreUpAsync(CancellationToken cancellationToken = default(CancellationToken))
        {
            return Task.CompletedTask;
        }

        public Task PostUpAsync(CancellationToken cancellationToken = default(CancellationToken))
        {
            return Task.CompletedTask;
        }

        public Task PreDownAsync(CancellationToken cancellationToken = default(CancellationToken))
        {
            return Task.CompletedTask;
        }

        public Task PostDownAsync(CancellationToken cancellationToken = default(CancellationToken))
        {
            return Task.CompletedTask;
        }
    }
}