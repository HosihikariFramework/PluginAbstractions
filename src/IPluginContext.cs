using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Hosihikari;

/// <summary>框架交给插件的一切（FX-0007）。能力面止于此，BDS 能力不在其中（FX-0001）。</summary>
public interface IPluginContext
{
    /// <summary>插件名，等于插件包目录名（FX-0006）。</summary>
    string Name { get; }

    /// <summary>插件包目录。</summary>
    string PackageDirectory { get; }

    /// <summary>插件自己写的运行期数据（FX-0018）。</summary>
    string DataDirectory { get; }

    /// <summary>分类名为插件名的日志器。</summary>
    ILogger Logger { get; }

    /// <summary>已按「包内默认 → 包外覆盖 → 环境变量」叠好的配置（FX-0018）。</summary>
    IConfiguration Configuration { get; }

    /// <summary>
    /// 事件总线。订阅由插件作用域持有，卸载时框架统一收回（FX-0007）；仍在 <c>OnUnload</c> 里显式退订
    /// 是好习惯，漏退不会把 ALC 钉死，但会让卸载多活一轮回收验证（FX-0004）。
    /// </summary>
    /// <remarks>
    /// 闸门在 <c>OnLoad</c> 期间是关着的（FX-0035），所以这里订阅的东西要等 <c>OnEnable</c>
    /// 成功返回之后才会开始收；在 <c>OnLoad</c> 里 <c>Publish</c> 也不会到达本插件的订阅者。
    /// 需要载入即处理的事件，在 <c>OnEnable</c> 里发。
    /// </remarks>
    IEventBus Events { get; }

    /// <summary>
    /// 调度器。跑在线程池上，不承诺游戏主线程（FX-0013）。
    /// </summary>
    /// <remarks>
    /// 与事件同一条闸门（FX-0035）：<c>OnLoad</c> 里排的定时器如果在启用前到期，那一次回调
    /// 不会补跑。要保证跑到，排到 <c>OnEnable</c> 之后。
    /// </remarks>
    IScheduler Scheduler { get; }

    /// <summary>
    /// 其他插件提供的服务（FX-0027）。插件之间共享类型的唯一通道 —— 它们不共享程序集，
    /// 直接引用另一个插件只会拿到自己包里的副本（FX-0026）。
    /// 你登记的东西在卸载时由框架收回。
    /// </summary>
    IPluginServiceRegistry PluginServices { get; }

    /// <summary>载体对象与载体事件的自愿取用出口（FX-0019）；与插件服务注册表分开。</summary>
    IServiceProvider CarrierServices { get; }

    /// <summary>
    /// 报告一个在钩子之外发生的故障（进程外环境死亡、协议错误等，FX-0030）。
    /// 宿主按故障处理：卸载该插件，成功后通知载体。进程内加载器通常用不到。
    /// </summary>
    /// <param name="diagnostic">诊断（FX-0059），进日志与父级的账。</param>
    void ReportFault(PluginDiagnostic diagnostic);
}
