# Hosihikari.Plugin.Templates

预编译星光框架插件的 `dotnet new` 模板。两个模板都会生成 .NET 类库、清单，以及调用框架打包器的打包脚本。

- **C#** —— 短名 `hosihikari-plugin`，入口用 `EntryPoint<T>` 声明。
- **F#** —— 短名 `hosihikari-plugin-fsharp`；F# 不能应用泛型特性，所以生成的项目用非泛型特性声明同一个
  `EntryPointAttributeBase`。

源码插件模板见
[CSharpPluginTemplate](https://github.com/HosihikariFramework/CSharpPluginTemplate)、
[FSharpPluginTemplate](https://github.com/HosihikariFramework/FSharpPluginTemplate) 与
[RpcPluginTemplate](https://github.com/HosihikariFramework/RpcPluginTemplate)。

## 构建并试用

```bash
dotnet pack Hosihikari.Plugin.Templates.csproj -c Release -o artifacts
dotnet new install artifacts/Hosihikari.Plugin.Templates.1.0.1.nupkg

dotnet new hosihikari-plugin -n MyPlugin -o MyPlugin
dotnet new hosihikari-plugin-fsharp -n MyFSharpPlugin -o MyFSharpPlugin
```

然后在并列的 `PluginManager` checkout 下分别构建并打包：

```bash
cd MyPlugin
bash ./pack.sh

cd ../MyFSharpPlugin
bash ./pack.sh
```

产物在 `dist/MyPlugin/` 与 `dist/MyFSharpPlugin/`，分别复制到 `<服务端>/plugins/<name>/`。

## 模板内容

C#（`Hosihikari.Plugin.Templates/`）：

- `Hosihikari.Plugin.Templates.csproj` —— net10.0 类库，引用契约时 `ExcludeAssets="runtime"`，共享层不会进包。
- `Plugin.cs` —— `IPlugin` 实现与 `EntryPoint<T>`。

F#（`Hosihikari.Plugin.Templates.FSharp/`）：

- `Hosihikari.Plugin.Templates.FSharp.fsproj` —— net10.0 F# 类库，同样引用契约。
- `Plugin.fs` —— `IPlugin` 实现与非泛型入口特性。

两个模板都带 `manifest.json`（`loader: assembly`）、`pack.ps1` / `pack.sh`（构建后调用插件管理器打包器的
`pack-plugin`，按规范剔除共享层）与双语插件 README。F# 模板会把 `FSharp.Core.dll` 一并放进插件包，
因为插件管理器的共享层不含它。

## 命名约束

项目名会成为程序集名、命名空间与清单名。请使用字母、数字与点；空格与连字符不是合法的 C# 或 F# 标识符。
清单里的 `name` 必须与包目录名一致。

## 许可

MIT-0，见 [LICENSE.md](LICENSE.md)。生成的项目引用的 `Hosihikari.Plugin.Abstractions` 1.0.0 契约为
MIT；0.7.0 包仍为 LGPL-3.0-or-later。
