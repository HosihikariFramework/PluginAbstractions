namespace Hosihikari;

/// <summary>诊断的严重级别（ADR-0059）。日志与 UI 按它决定呈现。</summary>
public enum PluginDiagnosticSeverity
{
    /// <summary>提示。</summary>
    Info,

    /// <summary>可疑，但可以继续。</summary>
    Warning,

    /// <summary>本次动作失败。</summary>
    Error,
}

/// <summary>
/// 一条机器可读的诊断（ADR-0059）：钩子拒绝、加载器预检与故障上报共用这一种形状。
/// 跨语言绑定只序列化一种记录；绑定里的 <see cref="Exception"/> 只带消息与类型名，不带堆栈。
/// </summary>
/// <param name="Code">机器可读的错误码，由报告方定义。</param>
/// <param name="Severity">严重级别。</param>
/// <param name="Message">可读的说明，进日志与父级的账。</param>
/// <param name="Exception">对应的异常；没有时为 <see langword="null"/>。</param>
public sealed record PluginDiagnostic(
    string Code,
    PluginDiagnosticSeverity Severity,
    string Message,
    Exception? Exception = null)
{
    /// <summary><see cref="PluginHookResult.Refuse(string)"/> 给只带原因文本的拒绝用的错误码。</summary>
    public const string RefusalCode = "refused";

    /// <summary>一条错误。</summary>
    public static PluginDiagnostic Error(string code, string message, Exception? exception = null) =>
        new(code, PluginDiagnosticSeverity.Error, message, exception);

    /// <summary>一条警告。</summary>
    public static PluginDiagnostic Warning(string code, string message) =>
        new(code, PluginDiagnosticSeverity.Warning, message);

    /// <summary>一条提示。</summary>
    public static PluginDiagnostic Info(string code, string message) =>
        new(code, PluginDiagnosticSeverity.Info, message);
}
