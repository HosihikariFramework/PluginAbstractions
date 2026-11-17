using Microsoft.Extensions.Logging;

namespace Hosihikari;

/// <summary>
/// 载体组件拿到的上下文（ADR-0061）：框架交给组件的输入面，方向与 ADR-0007 的宿主服务相反
/// （那边是框架给插件，这边是框架给组件）。给总线、日志与载体服务，不给插件面。
/// </summary>
public interface ICarrierComponentContext
{
    /// <summary>全局事件总线；组件用它把原生事件接进托管侧。</summary>
    IEventBus Events { get; }

    /// <summary>分类名为组件类型的日志器。</summary>
    ILogger Logger { get; }

    /// <summary>
    /// 载体服务（与 <see cref="IPluginContext.CarrierServices"/> 同一份）：内容由载体包提供，核心只透传。
    /// 组件的宿主句柄（例如 Endstone 的 <c>Plugin</c>/<c>Server</c>）从这里取，核心不认识它们。
    /// </summary>
    IServiceProvider CarrierServices { get; }
}
