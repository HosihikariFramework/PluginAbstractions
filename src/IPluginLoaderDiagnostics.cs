namespace Hosihikari;

/// <summary>
/// 加载器的可选预检接缝。管理器在 <see cref="IPluginLoader.CanLoad"/> 之后、<see cref="IPluginLoader.Load"/>
/// 之前调用它；问题并入加载报告，装载不会开始（ADR-0041）。
/// </summary>
public interface IPluginLoaderDiagnostics
{
    /// <summary>检查清单与包目录。返回空列表表示可以继续；只报告，不改变路由。</summary>
    /// <param name="manifest">包内清单。</param>
    /// <param name="packageDirectory">插件包目录。</param>
    /// <returns>可读的问题清单；为空表示没有发现问题。</returns>
    IReadOnlyList<string> Validate(PluginManifest manifest, string packageDirectory);
}
