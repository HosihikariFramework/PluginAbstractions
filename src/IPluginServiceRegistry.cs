namespace Hosihikari;

/// <summary>
/// 插件之间互相提供服务的通道（FX-0026 / 0027）。键就是服务类型，因此服务类型**必须来自
/// 共享层** —— 否则两边看到的是两个类型对象，取不到，而且不报错。
///
/// 登记在插件卸载时由框架收回，与事件订阅、定时器同一笔账（FX-0004）。
/// </summary>
public interface IPluginServiceRegistry
{
    /// <summary>
    /// 以 <typeparamref name="T"/> 为键登记一个服务。同类型已被登记时返回 <c>false</c>
    /// 并保留先来的那个 —— 冲突要让双方都看得见（FX-0009）。
    /// </summary>
    /// <typeparam name="T">服务类型，通常是一个放在共享层里的接口。</typeparam>
    /// <param name="service">实现实例。</param>
    /// <returns>登记成功为 <c>true</c>。</returns>
    bool Register<T>(T service) where T : class;

    /// <summary>取一个服务。没有登记过则返回 <c>null</c>。</summary>
    /// <typeparam name="T">服务类型。</typeparam>
    T? Get<T>() where T : class;
}
