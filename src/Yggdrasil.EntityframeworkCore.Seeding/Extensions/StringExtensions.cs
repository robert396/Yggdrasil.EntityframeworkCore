// ReSharper disable CheckNamespace

using System;
using System.Linq;

namespace Microsoft.EntityFrameworkCore.Extensions.Internal
{
    internal static class StringExtensions
    {
        internal static bool AreSame(this string migrationId, string comparison, bool useFullMigrationId, StringComparison comparer = StringComparison.InvariantCultureIgnoreCase)
        {
            return useFullMigrationId ? migrationId.Equals(comparison, comparer) : migrationId.StandardizeId().Equals(comparison.StandardizeId(), comparer);
        }

        internal static string StandardizeId(this string migrationId)
        {
            var parts = migrationId.Split('_');
            return parts.Length > 1 ? string.Join("_", parts.Skip(1)) : migrationId;
        }
    }
}