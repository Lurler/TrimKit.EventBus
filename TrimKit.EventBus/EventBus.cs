using System;
using System.Collections.Generic;
using System.Threading;

namespace Simple.EventBus;

/// <summary>
/// Simple event bus (aka event aggregator) for games.
/// - Mutation-safe during dispatch (so you can subscribe/unsubscribe at any time).
/// - Subscriptions return an <see cref="IDisposable"/> token to unsubscribe even without holding the original reference.
/// - It is safe to unsubscribe more than once.
/// </summary>
public sealed class EventBus : IEventBusSubscribe, IEventBusUnsubscribe, IEventBusPublish
{
    /// <summary>
    /// Static version of the EventBus if you don't want
    /// to create an instance yourself.
    /// </summary>
    public static EventBus Instance { get; } = new();

    private readonly Dictionary<Type, List<Delegate>> Handlers = new();

    private readonly object Lock = new();

    private sealed class SubscriptionToken(EventBus bus, Type eventType, Delegate handler) : IDisposable
    {
        private int disposed = 0; // 0 = not disposed, 1 = disposed

        public void Dispose()
        {
            if (Interlocked.Exchange(ref disposed, 1) == 0)
            {
                bus.Unsubscribe(eventType, handler);
            }
        }
    }

    /// <summary>
    /// Creates a subscription to a specific event type.
    /// Prevents duplicates (same target instance and method).
    /// Lambdas with new closures are treated as distinct handlers.
    /// </summary>
    public IDisposable Subscribe<T>(EventHandler<T> handler) where T : EventArgs
    {
        if (handler is null)
            throw new ArgumentNullException(nameof(handler));

        var eventType = typeof(T);

        lock (Lock)
        {
            if (!Handlers.TryGetValue(eventType, out List<Delegate>? delegates))
            {
                delegates = new();
                Handlers[eventType] = delegates;
            }

            // duplicate check
            foreach (var existing in delegates)
            {
                if (existing.Method == handler.Method && Equals(existing.Target, handler.Target))
                {
                    throw new InvalidOperationException($"Handler is already subscribed for event type {eventType.Name}.");
                }
            }

            delegates.Add(handler);

            return new SubscriptionToken(this, eventType, handler);
        }
    }

    /// <summary>
    /// Subscribe to an event to be notified only once when it's fired.
    /// </summary>
    public IDisposable SubscribeOnce<T>(EventHandler<T> handler) where T : EventArgs
    {
        if (handler is null)
            throw new ArgumentNullException(nameof(handler));

        IDisposable? token = null;
        EventHandler<T> wrapper = (s, e) =>
        {
            token?.Dispose();
            handler(s, e);
        };

        token = Subscribe(wrapper);
        return token;
    }

    /// <summary>
    /// Removes previously created subscription. Returns true if anything was actually removed or false otherwise.
    /// </summary>
    public bool Unsubscribe<T>(EventHandler<T> handler) where T : EventArgs
    {
        if (handler is null)
            throw new ArgumentNullException(nameof(handler));

        return Unsubscribe(typeof(T), handler);
    }

    private bool Unsubscribe(Type eventType, Delegate handler)
    {
        lock (Lock)
        {
            if (!Handlers.TryGetValue(eventType, out List<Delegate>? delegates))
                return false;
            var result = delegates.Remove(handler);
            if (delegates.Count == 0)
            {
                Handlers.Remove(eventType);
            }
            return result;
        }
    }

    /// <summary>
    /// Publishes an event to all subscribed handlers.
    /// </summary>
    public void Publish<T>(object? sender, T eventArgs) where T : EventArgs
    {
        if (eventArgs is null)
            throw new ArgumentNullException(nameof(eventArgs));

        // take snapshot of the current collection
        Delegate[]? snapshot;
        lock (Lock)
        {
            if (!Handlers.TryGetValue(typeof(T), out List<Delegate>? delegates) || delegates.Count == 0)
                return;
            snapshot = delegates.ToArray();
        }

        // use snapshot for iteration (outside of lock)
        foreach (var handler in snapshot)
        {
            ((EventHandler<T>)handler).Invoke(sender, eventArgs);
        }
    }

    /// <summary>
    /// Publishes an event to all subscribed handlers.
    /// </summary>
    public void Publish<T>(T eventArgs) where T : EventArgs
        => Publish(null, eventArgs);

    /// <summary>
    /// Returns the current subscriber count for <typeparamref name="T"/>.
    /// </summary>
    public int GetSubscriberCount<T>() where T : EventArgs
    {
        lock (Lock)
        {
            return Handlers.TryGetValue(typeof(T), out var list)
                ? list.Count : 0;
        }
    }

    /// <summary>
    /// Removes all subscriptions and resets the state of the event bus.
    /// </summary>
    public void Reset()
    {
        lock (Lock)
        {
            Handlers.Clear();
        }
    }
}
