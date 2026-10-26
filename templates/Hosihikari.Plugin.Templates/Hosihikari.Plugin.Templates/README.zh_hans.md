# Hosihikari.Plugin.Templates

一个预编译的星光框架插件。插件管理器内置的程序集加载器直接装载它，不需要额外的加载器包。

## 构建与打包

需要 .NET 10 SDK，以及并列的 `PluginManager` checkout（或设置
`HOSIHIKARI_PLUGINMANAGER`）。

```powershell
pwsh ./pack.ps1
```

```bash
bash ./pack.sh
```

打包器会剔除共享层，在 `dist/Hosihikari.Plugin.Templates/` 产出插件 DLL、私有依赖与
`manifest.json`。把这个目录复制到 `<服务端>/plugins/Hosihikari.Plugin.Templates/`。

## 命名

项目名会成为程序集名、命名空间与清单名。请使用合法的 C# 名称（字母、数字、点；不要空格或连字符）。
清单里的 `name` 必须与包目录名一致。

## 生命周期

`OnLoad` 在依赖就绪后执行；`OnEnable` 打开事件/定时器闸门，拒绝则保持已加载未启用；
`OnUnload` 释放插件持有的东西。钩子抛出异常属于故障，宿主会卸载插件。

需要源码编译的插件时，见
[CSharpPluginTemplate](https://github.com/HosihikariFramework/CSharpPluginTemplate)、
[FSharpPluginTemplate](https://github.com/HosihikariFramework/FSharpPluginTemplate) 与
[RpcPluginTemplate](https://github.com/HosihikariFramework/RpcPluginTemplate)；生命周期与打包见
在线作者文档。

## 许可

模板脚手架为 MIT-0，见 [LICENSE.md](LICENSE.md)。本项目引用的框架契约（Abstractions 1.0.0）为 MIT。
