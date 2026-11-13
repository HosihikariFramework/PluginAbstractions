# Hosihikari.Plugin.Abstractions

[English](README.md) | [简体中文](README.zh_hans.md)

The **plugin contract** of the Hosihikari framework: the few types a plugin implements, and
everything the framework hands to it.

The framework itself and the other packages live under
[HosihikariFramework](https://github.com/HosihikariFramework).

## Referencing it

```xml
<PackageReference Include="Hosihikari.Plugin.Abstractions" Version="[3.0.0]" ExcludeAssets="runtime" />
```

**`ExcludeAssets="runtime"` is not optional.** The contract lives in the **shared layer** — the
host has already loaded its copy. A second copy inside a plugin package is a second identity under
the same type name, and the plugin's casts will fail at run time, far away from the cause.

The same applies to any assembly the framework also ships (carrier bindings and the like).

## A plugin

```csharp
using Hosihikari;

[assembly: EntryPoint<MyPlugin>]

public sealed class MyPlugin : IPlugin
{
    public ValueTask OnLoad(IPluginContext context, CancellationToken cancellationToken)
    {
        context.Logger.LogInformation("loaded: {Dir}", context.PackageDirectory);
        return ValueTask.CompletedTask;
    }

    public ValueTask<PluginHookResult> OnEnable(CancellationToken cancellationToken) =>
        ValueTask.FromResult(PluginHookResult.Allow());

    public ValueTask<PluginHookResult> OnDisable(CancellationToken cancellationToken) =>
        ValueTask.FromResult(PluginHookResult.Allow());

    public ValueTask OnUnload(CancellationToken cancellationToken) => ValueTask.CompletedTask;
}
```

The entry point is declared by `[assembly: EntryPoint<T>]` on the assembly. Its generic constraint
is `where T : IPlugin, new()`, so both "the type implements the contract" and "it has a
parameterless constructor" are enforced by the compiler.

## Manifest

A plugin package is a directory; the directory name is the plugin name, and it must contain a
`manifest.json` (the format is LeviLamina's mod manifest; the framework's own fields live under
`extraInfo`). The model types live in the `Hosihikari.Plugin.Model` package, which the contract
references and the manager and packer share:

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

`dependencies` must include `hosihikari` so the parent loads the shim first. `type` is the parent loader's routing key and is always `hosihikari`. `extraInfo.loader` picks
the Loader that handles the package — **how to find the entry point inside the package is that
Loader's business**. `api_version` and `manifest_version` must equal `PluginContractInfo.CurrentApiVersion` and
`PluginContractInfo.CurrentManifestVersion`; `entry` must stay inside the package. A violation is rejected
**before** anything is loaded, with a readable report.

## Loader diagnostics

A loader can implement the optional `IPluginLoaderDiagnostics` companion interface. After
`CanLoad` selects the loader and before `Load` runs, the manager calls
`Validate(manifest, packageDirectory)`. A non-empty list becomes part of the load report and the
package is skipped; an empty list lets the load continue. Problems come back as `PluginDiagnostic`
records — the same shape a hook refusal and `ReportFault` carry.

```csharp
public IReadOnlyList<PluginDiagnostic> Validate(PluginManifest manifest, string packageDirectory)
{
    var entryPath = Path.Combine(packageDirectory, manifest.EntryPath);
    return File.Exists(entryPath)
        ? []
        : [PluginDiagnostic.Error("entry-missing", $"entry not found: {manifest.EntryPath}")];
}
```

The loader itself is a plugin package with `kind: loader`. It receives `ILoaderContext` — the plugin
context plus `RegisterLoader` — while ordinary plugins get the plain `IPluginContext`.

## Event priority

`Subscribe` takes an optional `EventPriority`: `Lowest`, `Low`, `Normal` (the default), `High`,
`Highest` and `Monitor`. Handlers run from `Lowest` to `Highest`, with `Monitor` last; handlers in
the same band run in subscription order. `Monitor` is an observation band by convention only —
don't mutate the event and don't cancel it. Priority orders managed handlers among themselves; it
does not change the order relative to native mods.

## Carrier components

A carrier package declares process-wide managed components under `carrier.components` in its
manifest. They implement `ICarrierComponent`: `StartAsync(ICarrierComponentContext, CancellationToken)`
and `StopAsync(CancellationToken)`, both called on the server thread. The context carries `Events`
and `Logger` only. Components exist once per process and survive every plugin disable and unload;
the event publisher and the server-thread `SynchronizationContext` install are the intended users.

## Status events

The manager publishes a `PluginStateChanged(Name, Previous, Current)` on the global bus for every
presence transition — `Loaded` → `Detached` on unload, and back when the plugin is loaded again. It
carries presence only; `Enabled` is the other axis and is not reported here. The record lives in the
shared layer, so the type matches exactly across ALCs and anything holding a bus subscription can
see unloads without being unloaded itself.

## Things an author should know

- **Hooks promise order and idempotence only** — not which server phase they land in. The world may
  not be loaded yet when `OnEnable` runs.
- **Hooks are async and take a `CancellationToken`.** Cancellation is cooperative: honour the token
  and return quickly. A cancelled `OnLoad` rolls the load back, a cancelled `OnEnable` keeps the
  plugin disabled, and a cancelled `OnDisable` / `OnUnload` means the framework stops waiting and
  records it.
- **The contract covers host services only**: logging, configuration, data directory, scheduling,
  the event bus, service resolution and the service registry. Worlds, entities, commands and game
  events are outside it.
- **Unloading must actually reclaim.** No long-lived static state, no bare threads. The framework
  reclaims event subscriptions and timers on unload, but that is a safety net, not a licence.
- **Disable closes the callback gate first.** The framework stops delivering events and timer
  callbacks before `OnDisable`; a refusal reopens it. In-flight callbacks finish; services and
  plugin-owned threads are outside this guarantee (ADR-0035).
- **Native libraries live with the process.** Put them at the package root or under
  `runtimes/<current portable RID>/native` (the RID directory wins). Unloading reclaims the managed
  ALC only; native modules and their static state survive reloads. Stop threads and unregister
  callbacks in `OnUnload`.
- Plugins do not share assemblies with each other: a direct reference gives you your own copy.
  Cross-plugin communication goes through the framework's channel, and the types on that channel
  must come from the shared layer.

## Plugin template

This repository also carries the `dotnet new` template for precompiled assembly plugins, published
as [Hosihikari.Plugin.Templates](https://www.nuget.org/packages/Hosihikari.Plugin.Templates). See
[`templates/Hosihikari.Plugin.Templates`](templates/Hosihikari.Plugin.Templates); source C#, F#, and
out-of-process plugins have their own template repositories:

- [CSharpPluginTemplate](https://github.com/HosihikariFramework/CSharpPluginTemplate)
- [FSharpPluginTemplate](https://github.com/HosihikariFramework/FSharpPluginTemplate)
- [RpcPluginTemplate](https://github.com/HosihikariFramework/RpcPluginTemplate)

The template scaffolding is MIT-0; the Abstractions source is MIT. The 0.7.0 package published on
NuGet before the switch remains LGPL-3.0-or-later; 0.7.1 and later are MIT.

## Licence

The Abstractions source is MIT, see [LICENSE.md](LICENSE.md). The 0.7.0 package published on NuGet
before the switch remains LGPL-3.0-or-later; 0.7.1 and later are MIT. The plugin templates under
`templates/` and the three template repositories are MIT-0.
