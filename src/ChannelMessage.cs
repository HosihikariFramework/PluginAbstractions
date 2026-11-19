namespace Hosihikari;

/// <summary>
/// 通道上对脚本友好的消息：主题 + 一段 JSON 载荷。
///
/// 它住在共享层，所以脚本与程序集插件看到的是**同一个** <see cref="Type"/> —— 这正是
/// FX-0026 说的"通道上的类型必须来自共享层"的具体答案。没有自己的载荷词汇时，脚本
/// 订不上任何事件；有了它，两边都能 <c>Publish&lt;ChannelMessage&gt;</c> 与 <c>Subscribe&lt;ChannelMessage&gt;</c>。
///
/// 载荷取 JSON 文本而不是结构化类型：它没有生命周期问题（<c>JsonElement</c> 脱离文档就不再有效），
/// 跨语言也没有歧义。程序集插件需要结构时自己解析。
/// </summary>
/// <param name="Topic">主题名，由发布者与订阅者自行约定。</param>
/// <param name="Json">载荷，一段 JSON 文本；没有载荷时用 <c>null</c> 或 <c>"null"</c>。</param>
public sealed record ChannelMessage(string Topic, string? Json);
