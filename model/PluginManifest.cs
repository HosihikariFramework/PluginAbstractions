using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Hosihikari;

/// <summary>
/// 插件清单 <c>hosihikari.json</c>（ADR-0058）。LeviLamina 路由用的 <c>manifest.json</c> 是打包器生成的信封。
/// 模型住在这里而不是契约程序集（ADR-0059），管理器与打包器共用同一份；契约引用本程序集。
///
/// 1.x 的 <c>extraInfo</c> 读法暂时保留：旧格式解析器还在，值优先取新字段。
/// </summary>
public sealed class PluginManifest
{
    /// <summary>清单 schema 版本，从 1 起（ADR-0058）；取代 1.x 的 <c>extraInfo.manifest_version</c>。</summary>
    [JsonPropertyName("schemaVersion")]
    public int? SchemaVersion { get; set; }

    /// <summary>插件名。必须等于插件包目录名。</summary>
    public string Name { get; set; } = "";

    /// <summary>包内入口文件的相对路径。</summary>
    [JsonPropertyName("entry")]
    public string EntryPath { get; set; } = "";

    /// <summary>插件自身版本。</summary>
    public string? Version { get; set; }

    /// <summary>处理这个包的加载器 id（ADR-0002）。不写就是 <c>assembly</c>。</summary>
    [JsonPropertyName("loader")]
    public string? LoaderName { get; set; }

    /// <summary>包类型（ADR-0006）：<c>plugin</c>（缺省）、<c>loader</c> 或 <c>carrier</c>。</summary>
    [JsonPropertyName("kind")]
    public string? Kind { get; set; }

    /// <summary>插件依赖的契约主版本（ADR-0012）。</summary>
    [JsonPropertyName("apiVersion")]
    public int? ApiVersion { get; set; }

    /// <summary>给父级与面板看的展示字段。</summary>
    public string? Description { get; set; }

    /// <summary>作者。</summary>
    public string? Author { get; set; }

    /// <summary>打包期设置。</summary>
    [JsonPropertyName("pack")]
    public ManifestPack? Pack { get; set; }

    /// <summary>载体组，只有 <c>kind: carrier</c> 的包有。</summary>
    [JsonPropertyName("carrier")]
    public ManifestCarrier? Carrier { get; set; }

    /// <summary>
    /// 父级路由键 <c>hosihikari</c>。信封里仍是 <c>type</c> 且必填；<c>hosihikari.json</c> 里不再有它，
    /// 但旧包手工搬家时可能留下，所以照旧认，写了就必须对。
    /// </summary>
    [JsonPropertyName("type")]
    public string? ParentName { get; set; }

    /// <summary>依赖的其它插件。加载按拓扑序、卸载按反序（ADR-0004）。</summary>
    public List<ManifestDependency> Dependencies
    {
        get => _dependencies;
        set => _dependencies = value ?? [];
    }

    /// <summary>
    /// 1.x 的扩展位，值保持原始 JSON。ADR-0058 之后只剩兼容读：新字段一律取顶层。
    /// </summary>
    public Dictionary<string, JsonElement> ExtraInfo
    {
        get => _extraInfo;
        set => _extraInfo = value ?? [];
    }

    private List<ManifestDependency> _dependencies = [];
    private Dictionary<string, JsonElement> _extraInfo = [];

    /// <summary>处理这个包的加载器 id（ADR-0002）。</summary>
    [JsonIgnore]
    public string Loader =>
        LoaderName ?? ExtraString("loader") ?? "assembly";

    /// <summary>契约主版本；新字段优先，1.x 的 <c>extraInfo.api_version</c> 兜底。</summary>
    [JsonIgnore]
    public int? EffectiveApiVersion => ApiVersion ?? ExtraInt("api_version");

    /// <summary>清单 schema 版本；新字段优先，1.x 的 <c>extraInfo.manifest_version</c> 兜底。</summary>
    [JsonIgnore]
    public int? EffectiveSchemaVersion => SchemaVersion ?? ExtraInt("manifest_version");

    /// <summary>包类型（ADR-0006）：<c>plugin</c>（缺省）、<c>loader</c> 或 <c>carrier</c>。</summary>
    [JsonIgnore]
    public string PackageKind => Kind ?? ExtraString("packageKind") ?? "plugin";

    private string? ExtraString(string key) =>
        ExtraInfo.TryGetValue(key, out var value) && value.ValueKind == JsonValueKind.String
            ? value.GetString()
            : null;

    // 1.x 把数字写成字符串（"3"），两种都收。
    private int? ExtraInt(string key) =>
        ExtraInfo.TryGetValue(key, out var value)
            ? value.ValueKind switch
            {
                JsonValueKind.Number when value.TryGetInt32(out var number) => number,
                JsonValueKind.String when int.TryParse(
                    value.GetString(), NumberStyles.Integer, CultureInfo.InvariantCulture, out var parsed) => parsed,
                _ => null,
            }
            : null;
}

/// <summary>打包期设置（ADR-0058）。</summary>
public sealed class ManifestPack
{
    /// <summary>进程外插件的启动命令；字符串或字符串数组都收（ADR-0031）。</summary>
    [JsonPropertyName("command")]
    public JsonElement? Command { get; set; }

    /// <summary>打包期要还原的第三方包，<c>Id/Version</c> 或 <c>Id</c>。</summary>
    [JsonPropertyName("packages")]
    public List<string> Packages
    {
        get => _packages;
        set => _packages = value ?? [];
    }

    /// <summary>打包期在包目录里执行的准备命令；字符串或字符串数组都收（ADR-0031）。</summary>
    [JsonPropertyName("prepack")]
    public JsonElement? Prepack { get; set; }

    private List<string> _packages = [];
}

/// <summary>载体组（ADR-0061）：只有载体包声明，核心据此装配进程级组件。</summary>
public sealed class ManifestCarrier
{
    /// <summary>载体带进共享层的程序集文件名。</summary>
    [JsonPropertyName("sharedAssemblies")]
    public List<string> SharedAssemblies
    {
        get => _sharedAssemblies;
        set => _sharedAssemblies = value ?? [];
    }

    /// <summary>进程级托管组件。</summary>
    [JsonPropertyName("components")]
    public List<ManifestCarrierComponent> Components
    {
        get => _components;
        set => _components = value ?? [];
    }

    private List<string> _sharedAssemblies = [];
    private List<ManifestCarrierComponent> _components = [];
}

/// <summary>一个载体组件：程序集文件名 + 类型全名。</summary>
public sealed class ManifestCarrierComponent
{
    /// <summary>组件所在的程序集文件名，必须在包内。</summary>
    [JsonPropertyName("assembly")]
    public string Assembly { get; set; } = "";

    /// <summary>实现 <c>ICarrierComponent</c> 的类型全名。</summary>
    [JsonPropertyName("type")]
    public string Type { get; set; } = "";
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
