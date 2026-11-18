using Microsoft.Extensions.DependencyInjection;

namespace Hosihikari;

/// <summary>
/// 配置器拿到的输入（ADR-0064）：容器、载体包目录与原生宿主句柄。
/// 目录让配置器能装包内的原生库（垫片）而不必猜进程状态；句柄交由载体包自己解释。
/// </summary>
public interface ICarrierServiceContext
{
    /// <summary>即将建成 <c>IServiceProvider</c> 的容器。</summary>
    IServiceCollection Services { get; }

    /// <summary>载体包目录（绝对路径），配置器可据此装载包内文件。</summary>
    string CarrierDirectory { get; }

    /// <summary>载体垫片传来的不透明句柄（例如 Endstone 的 <c>Plugin*</c>）；没有时为零。</summary>
    nint HostHandle { get; }
}
