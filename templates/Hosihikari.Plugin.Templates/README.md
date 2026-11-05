# Hosihikari.Plugin.Templates

`dotnet new` templates for precompiled Hosihikari plugins. Both templates generate a .NET class
library with a manifest and pack scripts that use the framework's packer.

- **C#** — short name `hosihikari-plugin`; entry declared with `EntryPoint<T>`.
- **F#** — short name `hosihikari-plugin-fsharp`; F# cannot apply the generic attribute, so the
  generated project declares the same `EntryPointAttributeBase` through a non-generic attribute.

For source-compiled plugins, see
[CSharpPluginTemplate](https://github.com/HosihikariFramework/CSharpPluginTemplate),
[FSharpPluginTemplate](https://github.com/HosihikariFramework/FSharpPluginTemplate), and
[RpcPluginTemplate](https://github.com/HosihikariFramework/RpcPluginTemplate).

## Build and try it

```bash
dotnet pack Hosihikari.Plugin.Templates.csproj -c Release -o artifacts
dotnet new install artifacts/Hosihikari.Plugin.Templates.1.1.0.nupkg

dotnet new hosihikari-plugin -n MyPlugin -o MyPlugin
dotnet new hosihikari-plugin-fsharp -n MyFSharpPlugin -o MyFSharpPlugin
```

Then build and pack each generated plugin:

```bash
cd MyPlugin
bash ./pack.sh

cd ../MyFSharpPlugin
bash ./pack.sh
```

The results are `dist/MyPlugin/` and `dist/MyFSharpPlugin/`; copy each directory to
`<server>/plugins/<name>/`.

## Template contents

C# (`Hosihikari.Plugin.Templates/`):

- `Hosihikari.Plugin.Templates.csproj` — net10.0 class library referencing the contract with
  `ExcludeAssets="runtime"`; the shared layer stays out of the package.
- `Plugin.cs` — `IPlugin` implementation and `EntryPoint<T>`.

F# (`Hosihikari.Plugin.Templates.FSharp/`):

- `Hosihikari.Plugin.Templates.FSharp.fsproj` — net10.0 F# library with the same contract reference.
- `Plugin.fs` — `IPlugin` implementation and a non-generic entry attribute.

Both templates also contain `manifest.json` (`loader: assembly`), `pack.ps1` / `pack.sh` (build,
then call the core packer's `pack-plugin` command), and a bilingual plugin README. The F# template
copies `FSharp.Core.dll` into the plugin package because the core shared layer does not contain it.

## Name constraints

The project name becomes the assembly name, namespace, and manifest name. Use letters, digits, and
dots; spaces and hyphens are not valid C# or F# identifiers. The manifest name must equal the
package directory name.

## Licence

MIT-0, see [LICENSE.md](LICENSE.md). The `Hosihikari.Plugin.Abstractions` 1.0.0 contract the generated
project references is MIT; the 0.7.0 package remains LGPL-3.0-or-later.
