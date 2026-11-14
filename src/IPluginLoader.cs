namespace Hosihikari;

/// <summary>某一插件种类的加载器（ADR-0002）。管理器只认这个契约，不认识 ALC 与脚本引擎（ADR-0010）。</summary>
public interface IPluginLoader
{
    /// <summary>清单 <c>extraInfo.loader</c> 的取值。</summary>
    string Id { get; }

    /// <summary>能不能处理这个包。管理器据此在多个加载器之间选型。</summary>
    /// <param name="manifest">包内清单。</param>
    /// <returns>能处理返回 <see langword="true"/>。</returns>
    bool CanLoad(PluginManifest manifest);

    /// <summary>把插件包变成可运行的实例。失败返回 <see langword="null"/>，不抛（ADR-0009）。</summary>
    /// <param name="manifest">包内清单。</param>
    /// <param name="packageDirectory">插件包目录。</param>
    /// <param name="context">交给插件的上下文。</param>
    /// <returns>实例与卸载句柄；加载失败返回 <see langword="null"/>。</returns>
    PluginHandle? Load(PluginManifest manifest, string packageDirectory, IPluginContext context);
}

/// <summary>加载器交出的插件实例与卸载能力。逻辑停用与真正卸载是两件事（ADR-0010）。</summary>
public abstract class PluginHandle(IPlugin plugin)
{
    /// <summary>
    /// 插件实例。卸载开始后置为 <see langword="null"/>，句柄本身不再强引用插件与它的搬运单元，
    /// 回收验证才有可能通过（ADR-0023）。
    /// </summary>
    public IPlugin? Plugin { get; protected set; } = plugin;

    /// <summary>搬运单元是否已经回收。卸载尚未开始时为 <see langword="false"/>。</summary>
    public abstract bool IsUnloaded { get; }

    /// <summary>
    /// 放下实例并开始回收。可重复调用；调用后 <see cref="Plugin"/> 不再持有引用。
    /// 真正的回收结果通过 <see cref="WaitForUnloadAsync"/> 或 <see cref="IsUnloaded"/> 观察。
    /// </summary>
    public abstract void Unload();

    /// <summary>
    /// 等待回收验证完成。调用方必须先丢掉自己持有的插件引用；返回是否为已回收。
    /// </summary>
    /// <param name="cancellationToken">取消等待。</param>
    /// <returns>搬运单元已经回收时为 <see langword="true"/>。</returns>
    public abstract Task<bool> WaitForUnloadAsync(CancellationToken cancellationToken = default);
}

/// <summary>一次卸载的失败发生在哪一段（ADR-0063）。钩子超时单独成类：它已经放下实例，只是没等钩子跑完。</summary>
public enum UnloadFailure
{
    /// <summary>没有失败。</summary>
    None,

    /// <summary>钩子的取消立了功：它被取消，或者干脆没在预算内返回。闸门与记账已按停用处理。</summary>
    HookTimedOut,

    /// <summary>钩子明确拒绝（<see cref="PluginHookResult.Allowed"/> 为假）；插件仍在册、可能活跃。</summary>
    HookRefused,

    /// <summary>钩子抛出了异常；按故障继续卸载。</summary>
    HookThrew,

    /// <summary>回收验证没在预算内通过；插件已放下，留在待回收，再卸一次会重新验证。</summary>
    ReclamationTimedOut,

    /// <summary>钩子超时之后回收验证也超时。</summary>
    HookAndReclamationTimedOut,

    /// <summary>回收验证做完了，结论是没回收掉。</summary>
    ReclamationFailed,
}

/// <summary>一次卸载的结果。失败要如实回传，不假装成功（ADR-0004 / ADR-0009）。</summary>
/// <param name="Released">
/// 是否真的回收掉了。失败可能是未加载、仍有依赖者、停用被拒绝（插件仍在册且可能活跃），
/// 也可能是已经放下但回收验证没通过（状态 Detached，再调一次卸载会重新验证）。
/// </param>
/// <param name="Error">失败原因；成功时为 <see langword="null"/>。</param>
/// <param name="Failure">失败落在哪一段（ADR-0063）；成功时为 <see cref="UnloadFailure.None"/>。</param>
/// <param name="HookTimedOut">
/// 钩子那一段是否被放弃过。它与 <see cref="Released"/> 正交：钩子超时之后回收验证仍可能通过，
/// 那时 <see cref="Released"/> 为真而这里也为真。
/// </param>
public readonly record struct UnloadResult(
    bool Released,
    string? Error,
    UnloadFailure Failure = UnloadFailure.None,
    bool HookTimedOut = false)
{
    /// <summary>成功：句柄已释放且回收已验证。</summary>
    /// <param name="hookTimedOut">钩子那一段是否超时被放弃过。</param>
    public static UnloadResult Ok(bool hookTimedOut = false) => new(true, null, UnloadFailure.None, hookTimedOut);

    /// <summary>失败：如实带上原因与失败的那一段。</summary>
    /// <param name="error">可读原因。</param>
    /// <param name="failure">失败发生在哪一段。</param>
    public static UnloadResult Failed(string error, UnloadFailure failure = UnloadFailure.ReclamationFailed) =>
        new(false, error, failure);
}
