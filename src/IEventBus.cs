namespace Hosihikari;

/// <summary>
/// 事件总线（ADR-0014）：在发布者线程上同步分发；单个 handler 抛异常被捕获、上报、不影响其余 handler。
/// 载体事件（ADR-0007 并入）也是原样透传，插件不必分场合记两套语义。
/// 订阅可带优先级（ADR-0060），分发按档从低到高、<see cref="EventPriority.Monitor"/> 最后；同档按订阅顺序。
/// </summary>
public interface IEventBus
{
    /// <summary>
    /// 订阅。返回的句柄用来退订 —— 忘了退订也不会锁死插件，框架在卸载时替你退（ADR-0007）。
    /// </summary>
    /// <param name="handler">收到事件时调用。抛出的异常会被捕获并上报，不会中断本次分发。</param>
    /// <param name="priority">
    /// 本订阅的档位，缺省 <see cref="EventPriority.Normal"/>。用 <see cref="EventPriority.Monitor"/> 时
    /// 约定只读：不修改事件对象、不取消。
    /// </param>
    /// <returns>退订句柄。可重复释放。</returns>
    IDisposable Subscribe<TEvent>(Action<TEvent> handler, EventPriority priority = EventPriority.Normal);

    /// <summary>在调用者的线程上同步分发给所有订阅者。</summary>
    /// <param name="event">
    /// 要分发的事件。按调用时指定的 <typeparamref name="TEvent"/> 精确匹配订阅者；
    /// 用基类发布不会命中按派生类型订阅的 handler。
    /// </param>
    void Publish<TEvent>(TEvent @event);
}

/// <summary>
/// 调度器（ADR-0007）。<c>ScheduleOnce</c> / <c>SchedulePeriodic</c> 跑在线程池上；
/// "在游戏主线程上跑"是载体能力，自持态拿不到（ADR-0013 / ADR-0019）。
/// </summary>
public interface IScheduler
{
    /// <summary>延迟一次。</summary>
    /// <param name="delay">延迟时长。</param>
    /// <param name="work">到点后在线程池上执行；抛出会被捕获并上报。</param>
    /// <returns>取消句柄。任务已开始或已结束时释放无副作用。</returns>
    IDisposable ScheduleOnce(TimeSpan delay, Action work);

    /// <summary>按间隔重复。</summary>
    /// <param name="interval">相邻两次的间隔。</param>
    /// <param name="work">每次在线程池上执行；抛出会被捕获并上报，不影响后续次数。</param>
    /// <returns>取消句柄。释放之后不再有新的调用，见实现说明的取消语义。</returns>
    IDisposable SchedulePeriodic(TimeSpan interval, Action work);
}
