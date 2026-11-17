namespace Hosihikari;

/// <summary>
/// 交给 <c>kind: loader</c> 的包的上下文（ADR-0059）：插件面之外多一个加载器注册。
/// 普通插件拿到的仍是 <see cref="IPluginContext"/>，看不到加载器动词。
/// </summary>
public interface ILoaderContext : IPluginContext
{
    /// <summary>
    /// 把一个加载器登记给管理器（ADR-0002）。加载器本身就是一个插件包，装载时用它把自己
    /// 交出去；**此后**装载的包才会被路由到它 —— 所以需要它的插件要在清单里声明依赖它（ADR-0006）。
    ///
    /// 登记之后不能撤销：它造出来的实例的类型住在它的装载上下文里。
    /// </summary>
    /// <param name="loader">要接管的插件种类。</param>
    /// <returns>登记成功为 <see langword="true"/>；同 id 已存在为 <see langword="false"/>。</returns>
    bool RegisterLoader(IPluginLoader loader);
}
