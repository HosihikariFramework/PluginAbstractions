namespace Hosihikari;

/// <summary>插件在管理器表里的在场状态（ADR-0025）。启用与停用是另一条轴。</summary>
public enum PluginState
{
    /// <summary>实例与作用域在场；插件可以接活或处于停用。</summary>
    Loaded,

    /// <summary>已放下实例与作用域，等待搬运单元回收；仍在管理器表中。</summary>
    Detached,
}

/// <summary>
/// 一次插件状态迁移（ADR-0062），发到全局总线。类型住共享层，跨 ALC 精确匹配才订得上（ADR-0026）。
/// </summary>
/// <param name="Name">插件名，等于包目录名。</param>
/// <param name="Previous">迁移前的状态。</param>
/// <param name="Current">迁移后的状态。</param>
public sealed record PluginStateChanged(string Name, PluginState Previous, PluginState Current);
