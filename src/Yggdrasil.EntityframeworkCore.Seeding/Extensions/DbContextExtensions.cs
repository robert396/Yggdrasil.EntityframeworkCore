// ReSharper disable CheckNamespace

using Microsoft.EntityFrameworkCore.Infrastructure;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Microsoft.EntityFrameworkCore.Extensions
{
    public static class DbContextExtensions
    {
        public static IEnumerable<Type> GetSeederTypes<TContext>(this TContext dbContext) where TContext : DbContext
        {
            return Assembly.GetEntryAssembly().GetReferencedAssemblies().Select(Assembly.Load).SelectMany(x => x.GetTypes().Where(SeederFilter));
        }

        public static IEnumerable<Type> GetSeedersForMigration<TContext>(this TContext dbContext, string migrationId) where TContext : DbContext
        {
            return (
                from seederType in dbContext.GetSeederTypes()
                let dbContextAttribute = seederType.GetCustomAttribute<DbContextAttribute>()
                    where dbContextAttribute != null && dbContextAttribute.ContextType == typeof(TContext)
                let migrationAttr = seederType.GetCustomAttribute<MigrationAttribute>()
                    where migrationAttr != null && migrationAttr.IsValid(migrationId)
                select seederType
            ).ToList();
        }

        private static bool SeederFilter(Type type)
        {
            return type.GetInterfaces().Contains(typeof(IDbContextSeeder)) && !type.IsInterface &&
                   type.GetCustomAttribute<MigrationAttribute>() != null &&
                   type.GetCustomAttribute<DbContextAttribute>() != null;
        }
    }
}