// ReSharper disable CheckNamespace

using System;
using Microsoft.EntityFrameworkCore.Extensions.Internal;

namespace Microsoft.EntityFrameworkCore
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = true)]
    public class SeederForMigrationAttribute : Attribute
    {
        private readonly string _migration;
        public bool UseFullMigrationId { get; set; } = false;
        public int Order { get; set; } = 0;

        public SeederForMigrationAttribute(string migration)
        {
            _migration = migration;
        }

        /// <summary>
        /// Checks to see if the given <paramref name="migrationId"/> is the same as the set migration id.
        /// </summary>
        /// <param name="migrationId"></param>
        /// <returns><c>True</c> if the migration ids are equal, <c>False</c> otherwise.</returns>
        public bool IsValid(string migrationId)
        {
            return migrationId.AreSame(_migration, UseFullMigrationId);
        }
    }
}