# Hosihikari.Plugin.Templates

A precompiled Hosihikari plugin. The core's built-in assembly loader loads it; no loader
package is required.

## Build and pack

Requires the .NET 10 SDK. Packing goes through the `Hosihikari.Plugin.Build` package; the manifest is
generated from the project.

```powershell
pwsh ./pack.ps1
```

```bash
bash ./pack.sh
```

The packer strips the shared layer and writes the package next to the project with the plugin DLL,
its private dependencies, and the generated `manifest.json`. Copy that directory to
`<server>/plugins/Hosihikari.Plugin.Templates/`.

## Naming

The project name becomes the assembly name, namespace, and manifest name. Use a valid C# name
(letters, digits, dots; no spaces or hyphens). The manifest name must match the package directory
name.

## Lifecycle

`OnLoad` runs after dependencies are ready. `OnEnable` opens the event/timer gate; a refusal keeps
the plugin loaded and disabled. `OnUnload` releases what the plugin owns. All four hooks are async
and take a `CancellationToken`. An exception from a hook is a fault and the host unloads the plugin.

For source-compiled plugins, see
[CSharpPluginTemplate](https://github.com/HosihikariFramework/CSharpPluginTemplate),
[FSharpPluginTemplate](https://github.com/HosihikariFramework/FSharpPluginTemplate), and
[RpcPluginTemplate](https://github.com/HosihikariFramework/RpcPluginTemplate); the online authoring
guide covers the lifecycle and packaging.

## Licence

The template scaffolding is MIT-0, see [LICENSE.md](LICENSE.md). The framework contract this
project references (Abstractions 3.0.0) is MIT.
