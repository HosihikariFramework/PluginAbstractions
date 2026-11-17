using Microsoft.Extensions.DependencyInjection;

namespace Hosihikari;

/// <summary>
/// 载体包的服务配置器（ADR-0064）：容器建成之前，由它把载体自己的服务注册进
/// <see cref="IServiceCollection"/>，插件与载体组件随后从 <see cref="IPluginContext.CarrierServices"/>
/// / <see cref="ICarrierComponentContext.CarrierServices"/> 取。载体专有的键类型由载体包（投影）定义，
/// 核心不认识它们；核心只负责调用本接口并交出容器。
/// </summary>
public interface ICarrierServicesConfigurator
{
    /// <summary>注册载体服务。在服务端线程上、任何插件装载之前调用一次。</summary>
    /// <param name="services">即将建成 <c>IServiceProvider</c> 的容器。</param>
    /// <param name="hostHandle">
    /// 载体垫片传来的不透明句柄（例如 Endstone 的 <c>Plugin*</c>）；没有时为零。
    /// 解释它的是载体包自己，核心只透传。
    /// </param>
    void Configure(IServiceCollection services, nint hostHandle);
}
