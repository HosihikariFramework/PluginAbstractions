namespace Hosihikari;

/// <summary>契约自身的版本（ADR-0012）。清单 <c>extraInfo.api_version</c> 必须等于它，否则拒载。</summary>
public static class PluginContractInfo
{
    /// <summary>当前的契约主版本。清单里的 <c>api_version</c> 与它不匹配的包在载入之前就会被剔除。</summary>
    public const string CurrentApiVersion = "3";

    /// <summary>当前的清单 schema 版本。清单里的 <c>manifest_version</c> 与它不匹配的包同样在载入之前被剔除。</summary>
    public const string CurrentManifestVersion = "1";

    /// <summary>
    /// 父级路由键的取值，也是清单顶层 <c>type</c> 该写的值（ADR-0006）。
    /// 它是父级命名空间里的一等名字：插件靠 <c>dependencies</c> 声明对它的依赖来保证排序。
    /// </summary>
    public const string ParentName = "hosihikari";

    /// <summary>
    /// 共享层的基地清单：契约、清单模型与契约引用的抽象包。载体声明的绑定程序集在载体包清单里，
    /// 由管理器并到这份基地清单之后（ADR-0059 / 0061），所以这里没有静态可变状态。
    /// 文件名同时用于加载期的身份判定与打包期的副本剔除。
    /// </summary>
    public static readonly string[] BaseSharedAssemblyFileNames =
    [
        "Hosihikari.Plugin.Abstractions.dll",
        "Hosihikari.Plugin.Model.dll",
        "Microsoft.Extensions.Configuration.Abstractions.dll",
        "Microsoft.Extensions.Logging.Abstractions.dll",
        "Microsoft.Extensions.DependencyInjection.Abstractions.dll",
        "Microsoft.Extensions.Primitives.dll",
    ];
}

/// <summary>
/// 一次生命周期钩子的结果（ADR-0030）。拒绝带诊断；钩子抛异常是故障，不走这里。
/// </summary>
/// <param name="Allowed">是否允许继续。</param>
/// <param name="Diagnostic">拒绝的诊断（ADR-0059）；允许时为 <see langword="null"/>。</param>
public readonly record struct PluginHookResult(bool Allowed, PluginDiagnostic? Diagnostic)
{
    /// <summary>拒绝原因的快捷读法；允许时为 <see langword="null"/>。</summary>
    public string? Reason => Diagnostic?.Message;

    /// <summary>允许：照常继续。</summary>
    public static PluginHookResult Allow() => new(true, null);

    /// <summary>拒绝：本次动作失败，原因交给父级与日志。</summary>
    /// <param name="reason">可读的原因；会包成 <see cref="PluginDiagnostic.RefusalCode"/> 的错误诊断，缺省则不带诊断。</param>
    public static PluginHookResult Refuse(string? reason = null) =>
        new(false, reason is null ? null : PluginDiagnostic.Error(PluginDiagnostic.RefusalCode, reason));

    /// <summary>拒绝：带上完整的诊断。与 <see cref="Refuse(string?)"/> 分开，免得 <c>Refuse(null)</c> 二义。</summary>
    public static PluginHookResult RefuseWith(PluginDiagnostic diagnostic) => new(false, diagnostic);
}

/// <summary>
/// 插件作者实现的契约（ADR-0008）。钩子只承诺顺序与幂等，不承诺与 BDS 阶段的对应（ADR-0013）。
/// 钩子异步并接收取消令牌（ADR-0059），取消是协作式的：插件应尽快尊重令牌返回。
/// </summary>
public interface IPlugin
{
    /// <summary>载入。此时依赖已就绪；服务器未必已就绪。取消按装载回滚处理，不留半加载状态。</summary>
    /// <param name="context">框架交给插件的一切。</param>
    /// <param name="cancellationToken">取消装载。</param>
    ValueTask OnLoad(IPluginContext context, CancellationToken cancellationToken);

    /// <summary>
    /// 启用：可以接活了。成功后打开作用域闸门；拒绝时留在册、未启用（ADR-0030 / 0035）。
    /// 取消时保持停用。
    /// </summary>
    /// <param name="cancellationToken">取消启用。</param>
    ValueTask<PluginHookResult> OnEnable(CancellationToken cancellationToken);

    /// <summary>
    /// 停用：停止接活。框架先关闭作用域闸门，不再投递事件与定时器回调；已在执行的回调允许跑完。
    /// 拒绝时闸门重开；卸载经停用，同样中止（ADR-0030 / 0035）。
    /// 父级态下由父级驱动；自持态正常退出时不会被调用。故障卸载与装载回滚仍会调用（ADR-0020 / 0030）。
    /// 取消表示框架不再等待，记账后继续后续阶段（ADR-0063）。
    /// </summary>
    /// <param name="cancellationToken">取消等待。</param>
    ValueTask<PluginHookResult> OnDisable(CancellationToken cancellationToken);

    /// <summary>
    /// 卸载：释放自己持有的东西。父级态下由父级驱动；自持态正常退出时不会被调用，
    /// 故障卸载与装载回滚仍会调用（ADR-0020 / 0030）—— 必须执行完的收尾不要只放在这里。
    /// 抛出按故障处理；取消表示框架不再等待，记账后继续（ADR-0063）。
    /// </summary>
    /// <param name="cancellationToken">取消等待。</param>
    ValueTask OnUnload(CancellationToken cancellationToken);
}
