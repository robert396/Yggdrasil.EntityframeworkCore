using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using System.Threading;
using System.Threading.Tasks;
using Yggdrasil.EntityframeworkCore.Seeding.Migrations.Contexts;

namespace Yggdrasil.EntityframeworkCore.Seeding.Migrations.Seeders
{
    [SeederForMigration("testMigration"), DbContext(typeof(SeederDbContext))]
    public class SeederWithoutInterface
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