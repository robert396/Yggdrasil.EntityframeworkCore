using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using System;
using System.Threading;
using System.Threading.Tasks;
using Yggdrasil.EntityframeworkCore.Seeding.Migrations.Contexts;
using Yggdrasil.EntityframeworkCore.Seeding.Migrations.Migrations;
using Yggdrasil.EntityframeworkCore.Seeding.Migrations.Models;

namespace Yggdrasil.EntityframeworkCore.Seeding.Migrations.Seeders
{
    [SeederForMigration(nameof(CreateUserSchema)), DbContext(typeof(SeederDbContext))]
    public class ValidSeeder : IDbContextSeeder
    {
        private readonly SeederDbContext _dbContext;

        public ValidSeeder(SeederDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public Task PreUpAsync(CancellationToken cancellationToken = default(CancellationToken))
        {
            return Task.CompletedTask;
        }

        public async Task PostUpAsync(CancellationToken cancellationToken = default(CancellationToken))
        {
            var user = new User
            {
                Id = Guid.NewGuid().ToString(),
                Username = "robertc",
                FirstName = "Robert",
                LastName = "Cloutman"
            };

            await _dbContext.Users.AddAsync(user, cancellationToken);
            var res = await _dbContext.SaveChangesAsync(cancellationToken);
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