using Microsoft.Extensions.Logging;

namespace Hosihikari;

/// <summary>
/// 载体组件拿到的上下文（ADR-0061）：框架交给组件的输入面，方向与 ADR-0007 的宿主服务相反
/// （那边是框架给插件，这边是框架给组件）。只管总线与日志，不给插件面。
/// </summary>
public interface ICarrierComponentContext
{
    /// <summary>全局事件总线；组件用它把原生事件接进托管侧。</summary>
    IEventBus Events { get; }

    /// <summary>分类名为组件类型的日志器。</summary>
    ILogger Logger { get; }
}
