using System.Text.Json;
using System.Text.Json.Serialization;

namespace Hosihikari;

/// <summary>
/// 插件清单（ADR-0006）。格式取 LeviLamina 的 <c>manifest.json</c>，我们自己的字段住在 <c>extraInfo</c>。
/// </summary>
public sealed class PluginManifest
{
    /// <summary>插件名。必须等于插件包目录名。</summary>
    public string Name { get; set; } = "";

    /// <summary>包内入口文件的相对路径。</summary>
    [JsonPropertyName("entry")]
    public string EntryPath { get; set; } = "";

    /// <summary>
    /// 父级的路由键，恒为 <see cref="PluginContractInfo.ParentName"/> —— 父级靠它把包交给我们（ADR-0006）。
    /// </summary>
    [JsonPropertyName("type")]
    public string ParentName { get; set; } = "";

    /// <summary>插件自身版本。</summary>
    public string? Version { get; set; }

    /// <summary>依赖的其它插件。加载按拓扑序、卸载按反序（ADR-0004）。</summary>
    public List<ManifestDependency> Dependencies
    {
        get => _dependencies;
        set => _dependencies = value ?? [];
    }

    /// <summary>
    /// 框架自己的字段都放这里 —— 清单里留给第三方的扩展位。
    /// 值保持原始 JSON：字符串、数组（例如进程外插件的 <c>command</c>）都放得下。
    /// 数组值只在自持态成立；父级 LeviLamina 的 extraInfo 是 string → string，数组必须在清单里编码成字符串。
    /// </summary>
    public Dictionary<string, JsonElement> ExtraInfo
    {
        get => _extraInfo;
        set => _extraInfo = value ?? [];
    }

    private List<ManifestDependency> _dependencies = [];
    private Dictionary<string, JsonElement> _extraInfo = [];

    /// <summary>处理这个包的加载器 id（ADR-0002）。不写就是 <c>assembly</c>。</summary>
    [JsonIgnore]
    public string Loader =>
        ExtraInfo.TryGetValue("loader", out var value) && value.ValueKind == JsonValueKind.String
            ? value.GetString() ?? "assembly"
            : "assembly";

    /// <summary>插件依赖的契约主版本（ADR-0012）；线上字段是 <c>extraInfo.api_version</c>。</summary>
    [JsonIgnore]
    public string? ApiVersion =>
        ExtraInfo.TryGetValue("api_version", out var value) && value.ValueKind == JsonValueKind.String
            ? value.GetString()
            : null;

    /// <summary>清单 schema 版本（ADR-0012）；线上字段是 <c>extraInfo.manifest_version</c>。</summary>
    [JsonIgnore]
    public string? ManifestVersion =>
        ExtraInfo.TryGetValue("manifest_version", out var value) && value.ValueKind == JsonValueKind.String
            ? value.GetString()
            : null;

    /// <summary>
    /// 包类型（ADR-0006）：<c>plugin</c>（缺省）、<c>loader</c> 或 <c>carrier</c>。
    /// 只有插件要求对框架的依赖；载体包按它跳过自持态扫描。
    /// </summary>
    [JsonIgnore]
    public string PackageKind =>
        ExtraInfo.TryGetValue("packageKind", out var value) && value.ValueKind == JsonValueKind.String
            ? value.GetString() ?? "plugin"
            : "plugin";
}

/// <summary>一条依赖声明。</summary>
public sealed class ManifestDependency
{
    /// <summary>被依赖的插件名。</summary>
    public string Name { get; set; } = "";

    /// <summary>
    /// 版本约束，使用 NuGet 的 SemVer 范围语法，例如 <c>1.2.3</c>（至少 1.2.3）、
    /// <c>[1.2.3]</c>（精确）或 <c>[1.0.0,2.0.0)</c>。省略、空字符串与 <c>*</c> 表示任意版本。
    /// </summary>
    [JsonPropertyName("version")]
    public string? VersionRange { get; set; }
}
