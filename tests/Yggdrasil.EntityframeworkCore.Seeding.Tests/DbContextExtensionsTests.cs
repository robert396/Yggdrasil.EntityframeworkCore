using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Extensions;
using Microsoft.Extensions.DependencyInjection;
using Shouldly;
using System.Linq;
using Microsoft.Data.Sqlite;
using Xunit;
using Yggdrasil.EntityframeworkCore.Seeding.Migrations.Contexts;
using Yggdrasil.EntityframeworkCore.Seeding.Migrations.Seeders;

namespace Yggdrasil.EntityframeworkCore.Seeding.Tests
{
    public class DbContextExtensionsTests
    {
        [Fact]
        public void GetSeederTypesShouldFindValidSeeders()
        {
            var connection = new SqliteConnection("DataSource=:memory:");
            connection.Open();

            var services = new ServiceCollection().AddDbContext<SeederDbContext>(opts => opts.UseSqlite(connection)).BuildServiceProvider();

            var dbContext = services.GetService<SeederDbContext>();
            var types = dbContext.GetSeederTypes().ToList();
            
            types.ShouldNotBeEmpty();
            types.ShouldContain(typeof(ValidSeeder));
            types.ShouldNotContain(typeof(SeederWithoutInterface));
            types.ShouldNotContain(typeof(SeederWithoutMigrationAttribute));
            types.ShouldNotContain(typeof(SeederWithoutDbContextAttribute));
        }
    }
}