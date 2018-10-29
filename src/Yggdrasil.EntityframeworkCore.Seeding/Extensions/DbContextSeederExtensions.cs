using System.Reflection;
using Microsoft.EntityFrameworkCore;

namespace Yggdrasil.EntityframeworkCore.Seeding.Extensions
{
    public static class DbContextSeederExtensions
    {
        public static int GetOrder(this IDbContextSeeder seeder)
        {
            return seeder.GetType().GetCustomAttribute<SeederForMigrationAttribute>()?.Order ?? 0;
        }
    }
}