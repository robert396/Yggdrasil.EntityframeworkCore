using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations.Internal;
using Microsoft.Extensions.DependencyInjection;
// ReSharper disable CheckNamespace

namespace Microsoft.EntityFrameworkCore
{
    public class SeederOptionsExtension : IDbContextOptionsExtension
    {
        public bool ApplyServices(IServiceCollection services)
        {
            services.AddScoped<Migrator>();
            return true;
        }

        public long GetServiceProviderHashCode()
        {
            return GetHashCode() * 397L;
        }

        public void Validate(IDbContextOptions options) { }

        public string LogFragment { get; } = "";
    }
}