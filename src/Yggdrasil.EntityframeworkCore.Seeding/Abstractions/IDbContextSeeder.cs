// ReSharper disable CheckNamespace

using System.Threading.Tasks;

namespace Microsoft.EntityFrameworkCore
{
    public interface IDbContextSeeder
    {
        /// <summary>
        /// Called before a migration is applied to the database.
        /// </summary>
        Task PreUpAsync();

        /// <summary>
        /// Called after a migration has been applied to the database.
        /// </summary>
        Task PostUpAsync();

        /// <summary>
        /// Called before a migration is removed from the database.
        /// </summary>
        Task PreDownAsync();

        /// <summary>
        /// Called after a migration has been removed from the database.
        /// </summary>
        Task PostDownAsync();
    }
}