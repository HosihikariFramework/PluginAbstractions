# Hosihikari.Plugin.Abstractions

[English](README.md) | [简体中文](README.zh_hans.md)

星光框架的**插件契约**：插件实现的那几个类型，以及它从框架手里拿到的东西。

框架本体与其余包见 [HosihikariFramework](https://github.com/HosihikariFramework)。

## 引用

```xml
<PackageReference Include="Hosihikari.Plugin.Abstractions" Version="[3.0.0]" ExcludeAssets="runtime" />
```

**`ExcludeAssets="runtime"` 不能省。** 契约驻留在**共享层** —— 宿主已经载入过这一份；插件包里若再带一份，就是同一个类型名下的第二个身份，插件转型会在运行期失败（而且报错点离病因很远）。

同理，插件引用任何"框架侧也有"的程序集（载体绑定等）时都要带上这一条。

## 一个插件

```csharp
using Hosihikari;

[assembly: EntryPoint<MyPlugin>]

public sealed class MyPlugin : IPlugin
{
    public ValueTask OnLoad(IPluginContext context, CancellationToken cancellationToken)
    {
        context.Logger.LogInformation("载入：{Dir}", context.PackageDirectory);
        return ValueTask.CompletedTask;
    }

    public ValueTask<PluginHookResult> OnEnable(CancellationToken cancellationToken) =>
        ValueTask.FromResult(PluginHookResult.Allow());

    public ValueTask<PluginHookResult> OnDisable(CancellationToken cancellationToken) =>
        ValueTask.FromResult(PluginHookResult.Allow());

    public ValueTask OnUnload(CancellationToken cancellationToken) => ValueTask.CompletedTask;
}
```

入口用程序集上的 `[assembly: EntryPoint<T>]` 声明。这个特性的泛型约束是 `where T : IPlugin, new()`，所以**类型实现了契约**与**有无参构造**两条都由编译器保证 —— 写错编译不过。

## 清单

插件包是一个目录，目录名就是插件名，里面必须有 `manifest.json`（格式沿用 LeviLamina 的 mod 清单，框架自己的字段住在 `extraInfo`）。清单模型类型住在 `Hosihikari.Plugin.Model` 包，契约引用它，管理器与打包器共用同一份：

```json
{
  "name": "My",
  "entry": "My.dll",
  "type": "hosihikari",
  "version": "1.0.0",
  "dependencies": [{ "name": "hosihikari" }],
  "extraInfo": {
    "api_version": "3",
    "manifest_version": "1",
    "loader": "assembly"
  }
}
```

`dependencies` 必须包含 `hosihikari`，父级才会先装寄宿垫片。`type` 是父级加载器的路由键，恒为 `hosihikari`；`extraInfo.loader` 决定由哪个加载器处理 —— **怎么从包里找到入口，是那个加载器自己的事**。`api_version` 与 `manifest_version` 必须分别等于 `PluginContractInfo.CurrentApiVersion` 与 `PluginContractInfo.CurrentManifestVersion`；`entry` 必须是包内相对路径。不满足的包在**载入之前**就会被剔除并给出可读报告。

## 加载器诊断

加载器可以实现可选的 `IPluginLoaderDiagnostics` 伴随接口。管理器在 `CanLoad` 选中加载器之后、
`Load` 开始之前调用 `Validate(manifest, packageDirectory)`；返回非空列表时，问题进入装载报告并
跳过该包，空列表则继续装载。问题以 `PluginDiagnostic` 记录返回 —— 与钩子拒绝、`ReportFault`
是同一种形状。

```csharp
public IReadOnlyList<PluginDiagnostic> Validate(PluginManifest manifest, string packageDirectory)
{
    var entryPath = Path.Combine(packageDirectory, manifest.EntryPath);
    return File.Exists(entryPath)
        ? []
        : [PluginDiagnostic.Error("entry-missing", $"入口不存在：{manifest.EntryPath}")];
}
```

加载器本身就是一个 `kind: loader` 的插件包，它拿到 `ILoaderContext` —— 插件上下文加上
`RegisterLoader`；普通插件拿到的仍是 `IPluginContext`。

## 事件优先级

`Subscribe` 可以带一个 `EventPriority`：`Lowest`、`Low`、`Normal`（缺省）、`High`、`Highest` 与
`Monitor`。处理器按 `Lowest` 到 `Highest` 的顺序执行，`Monitor` 最后；同一档内按订阅顺序。
`Monitor` 是约定上的观察档：不改事件、不取消。优先级只排托管处理器之间的顺序，不改变与原生 mod 的相对顺序。

## 载体组件

载体包在清单的 `carrier.components` 下声明进程级托管组件。它们实现 `ICarrierComponent`：
`StartAsync(ICarrierComponentContext, CancellationToken)` 与 `StopAsync(CancellationToken)`，都在服务端线程上调用，
上下文只给 `Events` 与 `Logger`。组件每进程恰好一份，不随任何插件的停用或卸载变化；事件发布器与服务端线程
`SynchronizationContext` 的安装是它的目标用户。

## 作者要知道的几条

- **钩子只承诺顺序与幂等**，不承诺它们落在服务器的哪个阶段 —— `OnEnable` 时世界未必已加载。
- **钩子是异步的，带 `CancellationToken`**。取消是协作式的：尊重令牌、尽快返回。`OnLoad` 被取消按装载回滚，`OnEnable` 被取消保持停用，`OnDisable` / `OnUnload` 被取消表示框架不再等待并记账。
- **契约只覆盖通用宿主服务**：日志、配置、数据目录、调度、事件总线、服务解析。世界、实体、命令、游戏事件不在其中。
- **卸载要真能回收**：不要留长期静态状态，不要裸起线程。事件订阅与定时器由框架在卸载时替你收回，但那只是兜底。
- **停用会先关回调闸门**：框架在 `OnDisable` 之前停止投递事件与定时器；拒绝停用则恢复。已在执行的回调会跑完，服务与插件自有线程不受管（ADR-0035）。
- **本机库随进程存活**：放在包根或 `runtimes/<当前可移植 RID>/native`（RID 目录优先）。卸载只回收托管 ALC；本机库与它的静态状态跨重载保留，线程与回调要在 `OnUnload` 里自己收尾。
- 插件之间不共享程序集：直接引用拿到的是自己包里的副本。跨插件通信走框架的通道，而通道上的类型必须来自共享层。

## 插件模板

本仓同时携带预编译程序集插件的 `dotnet new` 模板，发布为
[Hosihikari.Plugin.Templates](https://www.nuget.org/packages/Hosihikari.Plugin.Templates)，见
[`templates/Hosihikari.Plugin.Templates`](templates/Hosihikari.Plugin.Templates)；源码 C#、F# 与
进程外插件各有独立模板仓：

- [CSharpPluginTemplate](https://github.com/HosihikariFramework/CSharpPluginTemplate)
- [FSharpPluginTemplate](https://github.com/HosihikariFramework/FSharpPluginTemplate)
- [RpcPluginTemplate](https://github.com/HosihikariFramework/RpcPluginTemplate)

模板脚手架为 MIT-0；Abstractions 源码为 MIT。切换前已发布到 NuGet 的 0.7.0 包仍为
LGPL-3.0-or-later；0.7.1 及之后为 MIT。

## 许可

Abstractions 源码为 MIT，见 [LICENSE.md](LICENSE.md)。切换前已发布到 NuGet 的 0.7.0 包仍为
LGPL-3.0-or-later；0.7.1 及之后为 MIT。`templates/` 下的插件模板与三个模板仓为 MIT-0。
