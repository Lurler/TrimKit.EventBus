using System;

namespace TrimKit.EventBus;

/// <summary>
/// Interface that allows only unsubscription.
/// </summary>
public interface IEventBusUnsubscribe
{
    bool Unsubscribe<T>(EventHandler<T> handler) where T : EventArgs;
}