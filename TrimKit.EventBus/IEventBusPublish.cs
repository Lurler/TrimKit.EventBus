using System;

namespace TrimKit.EventBus;

/// <summary>
/// Interface that allows only publishing new events.
/// </summary>
public interface IEventBusPublish
{
    void Publish<T>(object? sender, T eventArgs) where T : EventArgs;
}