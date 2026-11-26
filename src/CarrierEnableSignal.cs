namespace Hosihikari;

/// <summary>Announces that the carrier plugin itself has been enabled.</summary>
/// <remarks>
/// The carrier is enabled <em>after</em> the child plugins it loaded, so a capability component
/// that only watches a poll timer attaches too late — the children's <c>OnEnable</c> has already
/// returned, and they read every capability as unavailable. This is raised from the carrier's
/// <c>onEnable</c>, on the server thread, after Endstone has set the enabled flag, which is the
/// first instant registrations are accepted at all.
/// <para>
/// Progress is also recorded for late starters: a component whose assembly only loads after the
/// enable can still read <see cref="HasRaised"/> and start immediately instead of waiting for an
/// event it will never see.
/// </para>
/// </remarks>
public static class CarrierEnableSignal
{
    /// <summary>
    /// The handlers, swapped as a whole rather than mutated.
    /// </summary>
    /// <remarks>
    /// A plain <c>event</c> would lose subscribers: <c>+=</c> is a read-modify-write, so a
    /// subscription that lands while <see cref="Signal"/> is taking its snapshot can be dropped,
    /// and <see cref="HasRaised"/> cannot cover it — the gate's own subscription is the thing that
    /// went missing, not its reaction to one that already ran. Components subscribe from the
    /// startup thread while the carrier raises the signal, so the two really do overlap.
    /// </remarks>
    private static Action[] s_handlers = [];

    /// <summary>Subscribes a handler; runs on the server thread.</summary>
    public static void Subscribe(Action handler)
    {
        ArgumentNullException.ThrowIfNull(handler);

        Action[] current, updated;
        do
        {
            current = Volatile.Read(ref s_handlers);
            updated = [.. current, handler];
        }
        while (Interlocked.CompareExchange(ref s_handlers, updated, current) != current);
    }

    /// <summary>Removes a previously subscribed handler.</summary>
    public static void Unsubscribe(Action handler)
    {
        ArgumentNullException.ThrowIfNull(handler);

        Action[] current, updated;
        do
        {
            current = Volatile.Read(ref s_handlers);
            var index = Array.IndexOf(current, handler);
            if (index < 0)
            {
                return;
            }

            updated = [.. current[..index], .. current[(index + 1)..]];
        }
        while (Interlocked.CompareExchange(ref s_handlers, updated, current) != current);
    }

    /// <summary>Whether the carrier has already been enabled in this host.</summary>
    /// <remarks>Latched for the whole process: the carrier does not have a disable path that
    /// would make a reset correct, and a stale <see langword="false"/> is the worse failure.</remarks>
    public static bool HasRaised => Volatile.Read(ref s_hasRaised);

    private static bool s_hasRaised;

    /// <summary>Raises the signal. Called by the carrier from <c>onEnable</c>.</summary>
    /// <remarks>
    /// The two steps are in a fixed order and the order is part of the contract. The flag has to
    /// be published <em>before</em> the handler snapshot is taken: a subscriber that lands in
    /// between is served by the <see cref="HasRaised"/> backstop instead of being called twice,
    /// but only if it can actually see the flag. A plain write followed by a read may sit in the
    /// store buffer past the read on x86, which leaves that subscriber missing from the snapshot
    /// and still reading <see langword="false"/> — the one gap neither path covers. Do not swap
    /// the two statements for each other, and keep both volatile.
    /// </remarks>
    public static void Signal()
    {
        Volatile.Write(ref s_hasRaised, true);

        var snapshot = Volatile.Read(ref s_handlers);

        // One snapshot for the whole raise.
        foreach (var handler in snapshot)
        {
            handler();
        }
    }
}
