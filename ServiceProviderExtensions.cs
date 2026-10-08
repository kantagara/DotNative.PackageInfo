using System;
using Microsoft.Extensions.DependencyInjection;

namespace DotNative.PackageInfo;

public static class PackageInfoServiceProviderExtensions
{
#if NET10_0_OR_GREATER
    extension(IServiceProvider services)
    {
        /// <summary>Resolves the registered plugin using the provider's DI lifetime.</summary>
        public IPackageInfo PackageInfo => services.GetRequiredService<IPackageInfo>();
    }
#else
    /// <summary>Resolves the registered plugin using the provider's DI lifetime.</summary>
    public static IPackageInfo PackageInfo(this IServiceProvider services) =>
        services.GetRequiredService<IPackageInfo>();
#endif
}
