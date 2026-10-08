# DotNative.PackageInfo

Reads version and display metadata embedded in the running .NET app. This is a
managed DotNative plugin and works on the current .NET runtime target. It does
not query the Android PackageManager or Apple bundle at runtime.

```csharp
builder.Services.AddPackageInfo();
var info = services.PackageInfo.GetCurrent();
Console.WriteLine($"{info.DisplayName} {info.Version} ({info.BuildNumber})");
```

`PackageName` comes from the entry assembly name unless the app supplies
`AssemblyMetadata("DotNativePackageName", "...")`. `DisplayName` checks
`DotNativeDisplayName`, assembly title, then assembly product. `Version` uses
`AssemblyInformationalVersion` and falls back to file/assembly version. The build
number comes from the executable file version, then assembly version.

Package identifiers and store-managed version/build metadata are not yet read
from Android/iOS bundles.
For NativeAOT apps, keep the entry assembly's version attributes in the publish
output. This independently authored DotNative implementation is MIT licensed.

## Service access

Import `DotNative.PackageInfo` to access the plugin through `IServiceProvider`:

```csharp
using DotNative.PackageInfo;

var plugin = services.PackageInfo;
```

The getter calls `GetRequiredService<IPackageInfo>()` on every access, preserving
DI lifetimes and the usual missing-registration error. Register the plugin with
`AddPackageInfo(...)` before building the provider.

A `net10.0` application uses the property syntax with C# 14 or later. A
`net9.0` application uses only the method equivalent:

```csharp
var plugin = services.PackageInfo();
```

The package contains separate `net9.0` and `net10.0` assemblies. NuGet selects
the assembly matching the application target framework. `NET10_0_OR_GREATER`
selects the property; the `#else` branch selects the method.

Build and pack both targets with .NET 10 SDK. A source build using .NET 9 SDK
builds only `net9.0`; it does not produce the .NET 10 assembly.
