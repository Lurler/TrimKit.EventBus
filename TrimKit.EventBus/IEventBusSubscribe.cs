using System;

namespace Simple.EventBus;

/// <summary>
/// Interface that allows only subscription.
/// </summary>
public interface IEventBusSubscribe
{
    IDisposable Subscribe<T>(EventHandler<T> handler) where T : EventArgs;
}