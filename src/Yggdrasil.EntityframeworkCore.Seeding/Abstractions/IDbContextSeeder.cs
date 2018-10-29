// ReSharper disable CheckNamespace

using System.Threading;
using System.Threading.Tasks;

namespace Microsoft.EntityFrameworkCore
{
    public interface IDbContextSeeder
    {
        /// <summary>
        /// Called before a migration is applied to the database.
        /// </summary>
        Task PreUpAsync(CancellationToken cancellationToken = default(CancellationToken));

        /// <summary>
        /// Called after a migration has been applied to the database.
        /// </summary>
        Task PostUpAsync(CancellationToken cancellationToken = default(CancellationToken));

        /// <summary>
        /// Called before a migration is removed from the database.
        /// </summary>
        Task PreDownAsync(CancellationToken cancellationToken = default(CancellationToken));

        /// <summary>
        /// Called after a migration has been removed from the database.
        /// </summary>
        Task PostDownAsync(CancellationToken cancellationToken = default(CancellationToken));
    }
}