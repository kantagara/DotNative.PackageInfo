using System.Diagnostics;
using System.Reflection;
using System.Runtime.Versioning;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace DotNative.PackageInfo;

public sealed record AppPackageInfo(
    string PackageName,
    string Version,
    string BuildNumber,
    string? DisplayName,
    string? InformationalVersion
);

public interface IPackageInfo
{
    AppPackageInfo GetCurrent();
}

/// <summary>Reads metadata embedded in the .NET entry assembly and executable.</summary>
public sealed class RuntimePackageInfo : IPackageInfo
{
    public AppPackageInfo GetCurrent()
    {
        var assembly = Assembly.GetEntryAssembly() ?? Assembly.GetExecutingAssembly();
        var name = assembly.GetName();
        var filePath = Environment.ProcessPath;
        FileVersionInfo? file = null;
        if (!string.IsNullOrWhiteSpace(filePath))
        {
            try
            {
                file = FileVersionInfo.GetVersionInfo(filePath);
            }
            catch (FileNotFoundException) { }
            catch (UnauthorizedAccessException) { }
        }

        var attributes = assembly
            .GetCustomAttributes<AssemblyMetadataAttribute>()
            .Where(a => a.Value is not null)
            .ToDictionary(a => a.Key, a => a.Value!, StringComparer.Ordinal);
        var title = assembly.GetCustomAttribute<AssemblyTitleAttribute>()?.Title;
        var product = assembly.GetCustomAttribute<AssemblyProductAttribute>()?.Product;
        var version =
            assembly
                .GetCustomAttribute<AssemblyInformationalVersionAttribute>()
                ?.InformationalVersion
            ?? file?.ProductVersion
            ?? name.Version?.ToString()
            ?? "0.0.0";
        var build = file?.FileVersion ?? name.Version?.ToString() ?? "0";
        var packageName =
            attributes.GetValueOrDefault("DotNativePackageName")
            ?? name.Name
            ?? Path.GetFileNameWithoutExtension(filePath)
            ?? "unknown";
        var displayName =
            attributes.GetValueOrDefault("DotNativeDisplayName")
            ?? NonEmpty(title)
            ?? NonEmpty(product);
        return new(packageName, version, build, displayName, version);
    }

    private static string? NonEmpty(string? value) =>
        string.IsNullOrWhiteSpace(value) ? null : value;
}

public static class PackageInfoServices
{
    public static IServiceCollection AddPackageInfo(this IServiceCollection services)
    {
        services.TryAddSingleton<IPackageInfo, RuntimePackageInfo>();
        return services;
    }
}
