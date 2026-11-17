namespace Hosihikari.Plugin.Templates.FSharp

open System.Threading
open System.Threading.Tasks
open Hosihikari
open Microsoft.Extensions.Logging

/// <summary>A precompiled plugin loaded by the core's built-in assembly loader.</summary>
type Plugin() =
    interface IPlugin with
        member _.OnLoad(context: IPluginContext, _cancellationToken: CancellationToken) =
            context.Logger.LogInformation("loaded from {Directory}", context.PackageDirectory)
            ValueTask.CompletedTask

        member _.OnEnable(_cancellationToken: CancellationToken) =
            ValueTask.FromResult(PluginHookResult.Allow())

        member _.OnDisable(_cancellationToken: CancellationToken) =
            ValueTask.FromResult(PluginHookResult.Allow())

        member _.OnUnload(_cancellationToken: CancellationToken) = ValueTask.CompletedTask

/// <summary>
/// Declares the entry type. F# cannot apply the generic EntryPoint&lt;T&gt; attribute,
/// so the plugin supplies the same EntryPointAttributeBase through a non-generic attribute.
/// </summary>
type PluginEntryPointAttribute() =
    inherit EntryPointAttributeBase()

    override _.CreateInstance() = Plugin() :> IPlugin

[<assembly: PluginEntryPoint>]
do ()
