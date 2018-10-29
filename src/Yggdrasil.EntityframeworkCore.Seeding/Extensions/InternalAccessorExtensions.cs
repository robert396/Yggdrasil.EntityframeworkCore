using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Linq;
// ReSharper disable CheckNamespace

namespace Microsoft.EntityFrameworkCore.Internal
{
    /// <summary>
    /// Adapted from <example>https://github.com/aspnet/EntityFrameworkCore/blob/d59be61006d78d507dea07a9779c3c4103821ca3/src/EFCore/Internal/InternalAccessorExtensions.cs</example>
    /// </summary> 
    public static class InternalAccessorExtensions
    {
        public static object GetService(this IInfrastructure<IServiceProvider> accessor, Type serviceType)
        {
            if (accessor == null)
            {
                return null;
            }

            var internalServiceProvider = accessor.Instance;

            var service = internalServiceProvider.GetService(serviceType) ?? internalServiceProvider
                                 .GetService<IDbContextOptions>()?.Extensions.OfType<CoreOptionsExtension>()
                                 .FirstOrDefault()?.ApplicationServiceProvider?.GetService(serviceType);

            if (service == null)
            {
                throw new InvalidOperationException(CoreStrings.NoProviderConfiguredFailedToResolveService(serviceType.DisplayName()));
            }

            return service;
        }
    }
}