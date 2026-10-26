# Hosihikari.Plugin.Templates.FSharp

一个预编译的 F# 星光框架插件。插件管理器内置的程序集加载器直接装载它，不需要额外的加载器包。

## 构建与打包

需要 .NET 10 SDK，以及同级的 `PluginManager` 检出（或用 `HOSIHIKARI_PLUGINMANAGER` 指向一个）。

```powershell
pwsh ./pack.ps1
```

```bash
bash ./pack.sh
```

打包器会剔除共享层，并把插件 DLL、私有依赖与 `manifest.json` 写到
`dist/Hosihikari.Plugin.Templates.FSharp/`。把该目录复制到
`<服务端>/plugins/Hosihikari.Plugin.Templates.FSharp/`。

## 命名

项目名会成为程序集名、命名空间与清单名。使用合法的 F# 名称（字母、数字、点；不要空格或连字符）。
清单里的 name 必须与包目录名一致。

## 生命周期

`OnLoad` 在依赖就绪后执行；`OnEnable` 打开事件/定时器闸门，拒绝则保持已加载未启用；
`OnUnload` 释放插件持有的东西。钩子抛出异常属于故障，宿主会卸载插件。

## 许可

模板脚手架为 MIT-0，见 [LICENSE.md](LICENSE.md)。本项目引用的框架契约（Abstractions 1.0.0）为 MIT。
