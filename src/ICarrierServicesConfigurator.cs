using Microsoft.Extensions.DependencyInjection;

namespace Hosihikari;

/// <summary>
/// 载体包的服务配置器（ADR-0064）：容器建成之前，由它把载体自己的服务注册进
/// <see cref="ICarrierServiceContext.Services"/>，插件与载体组件随后从
/// <see cref="IPluginContext.CarrierServices"/> / <see cref="ICarrierComponentContext.CarrierServices"/> 取。
/// 载体专有的键类型由载体包（投影）定义，核心不认识它们；核心只负责调用本接口并交出容器。
/// </summary>
public interface ICarrierServicesConfigurator
{
    /// <summary>注册载体服务。在服务端线程上、任何组件与插件装载之前调用一次。</summary>
    /// <param name="context">容器、载体包目录与宿主句柄。</param>
    void Configure(ICarrierServiceContext context);
}
