namespace Hosihikari;

/// <summary>
/// 订阅优先级（ADR-0060）。分发按 <see cref="Lowest"/> → <see cref="Highest"/> 的顺序，
/// <see cref="Monitor"/> 永远最后；同一档内按订阅顺序。
///
/// 优先级只决定托管处理器之间的相对顺序：载体侧对每个原生事件只注册固定优先级的一个原生监听器，
/// 档位不改变与其它原生 mod 的相对顺序。
/// </summary>
public enum EventPriority
{
    /// <summary>最先收到。</summary>
    Lowest = -2,

    /// <summary>早于默认档。</summary>
    Low = -1,

    /// <summary>默认档。</summary>
    Normal = 0,

    /// <summary>晚于默认档。</summary>
    High = 1,

    /// <summary>最后一批可改动的处理器。</summary>
    Highest = 2,

    /// <summary>
    /// 只读观察档，在所有其它档之后收到。**约定上不得修改事件对象、不得取消**；
    /// 事件对象是可变 class，这里只有约定、没有第二套订阅 API，也不做运行期强制。
    /// 违反约定不会被拦截，只会让后面没有别的档来纠错。
    /// </summary>
    Monitor,
}
