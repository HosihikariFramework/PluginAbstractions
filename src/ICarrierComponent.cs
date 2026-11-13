using Microsoft.Extensions.Logging;

namespace Hosihikari;

/// <summary>
/// 载体组件（ADR-0061）：载体包在 <c>hosihikari.json</c> 的 <c>carrier.components</c> 声明的进程级托管组件。
/// 核心构造并持有它，每进程一份，不随任何插件的停用或卸载变化。
/// 事件发布器与服务端线程 <c>SynchronizationContext</c> 的安装都属于它。
/// </summary>
public interface ICarrierComponent
{
    /// <summary>
    /// 启动。在事件分发对插件开启**之前**执行；任何一个组件启动失败，整个启动序列按失败处理。
    /// 在服务端线程上调用 —— 只在该线程生效的 <c>SynchronizationContext</c> 必须在它上面安装，投影侧补救不了。
    /// </summary>
    /// <param name="context">可用的总线与日志。</param>
    /// <param name="token">取消启动。</param>
    ValueTask StartAsync(ICarrierComponentContext context, CancellationToken token);

    /// <summary>
    /// 停止。在事件分发停止**之前**执行；令牌按管理器的停止预算取消，超时后框架不再等待并记账，继续进程退出。
    /// 同样在服务端线程上调用，用来撤下在 <see cref="StartAsync"/> 里安装的东西。
    /// </summary>
    /// <param name="token">取消停止。</param>
    ValueTask StopAsync(CancellationToken token);
}

/// <summary>载体组件拿到的上下文（ADR-0061）。只管总线与日志，不给插件面。</summary>
public interface ICarrierComponentContext
{
    /// <summary>全局事件总线；组件用它把原生事件接进托管侧。</summary>
    IEventBus Events { get; }

    /// <summary>分类名为组件类型的日志器。</summary>
    ILogger Logger { get; }
}
