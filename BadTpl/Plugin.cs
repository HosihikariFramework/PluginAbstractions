using Hosihikari;
using Microsoft.Extensions.Logging;

[assembly: EntryPoint<BadTpl.Plugin>]

namespace BadTpl;

/// <summary>A precompiled plugin loaded by the core's built-in assembly loader.</summary>
public sealed class Plugin : IPlugin
{
    public ValueTask OnLoad(IPluginContext context, CancellationToken cancellationToken)
    {
        context.Logger.LogInformation(
            "{Name} loaded from {Directory}",
            context.Name,
            context.PackageDirectory);
        return ValueTask.CompletedTask;
    }

    public ValueTask<PluginHookResult> OnEnable(CancellationToken cancellationToken) =>
        ValueTask.FromResult(PluginHookResult.Allow());

    public ValueTask<PluginHookResult> OnDisable(CancellationToken cancellationToken) =>
        ValueTask.FromResult(PluginHookResult.Allow());

    public ValueTask OnUnload(CancellationToken cancellationToken) => ValueTask.CompletedTask;
}
