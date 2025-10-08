# TrimKit.EventBus

A lightweight and mutation-safe event bus (event aggregator) for C# applications and games. Designed for simplicity and ease of use.

TrimKit.EventBus allows different parts of your system to communicate via strongly-typed events without hard dependencies or direct references.

## Features
 - Lightweight & dependency-free.
 - Thread-safe.
 - Mutation-safe - handlers can safely modify subscriptions even during event publishing.
 - Disposable subscriptions - each Subscribe() returns an IDisposable token for easy unsubscribe.
 - SubscribeOnce() support - automatically unsubscribes after first event.
 - Duplicate protection - prevents duplicate handler registration.

## Installation
Use provided nuget package or download the source.

[![NuGet](https://img.shields.io/nuget/v/TrimKit.EventBus.svg?style=for-the-badge)](https://www.nuget.org/packages/TrimKit.EventBus)

:wrench: `dotnet add package TrimKit.EventBus`

## Quick start

Define your events, they must inherit from ``EventArgs``:
```cs
public sealed class GameStartedEvent : EventArgs
{
    public DateTime StartTime { get; }
    public GameStartedEvent(DateTime startTime) => StartTime = startTime;
}

public sealed class CharacterDamagedEvent : EventArgs
{
    public string Name { get; }
    public int Damage { get; }
    public CharacterDamagedEvent(string name, int damage)
    {
        Name = name;
        Damage = damage;
    }
}
```

Create the event bus itself:
```cs
var bus = new EventBus();
```

Add subscribers. You can subscribe normally or use ``SubscribeOnce()`` for a one-time handler.
```cs
// normal subscription
var token = bus.Subscribe<CharacterDamagedEvent>((s, e) =>
{
    Console.WriteLine($"{e.Name} took {e.Damage} damage!");
});

// subscribe once
bus.SubscribeOnce<GameStartedEvent>((s, e) =>
{
    Console.WriteLine($"[Once] Game started at {e.StartTime:T}");
});
```

Each ``Subscribe()`` returns an ``IDisposable`` token you can later call ``Dispose()`` on to unsubscribe:
```cs
token.Dispose();
```

And now you can start publishing events:
```cs
bus.Publish(new GameStartedEvent(DateTime.Now));
bus.Publish(null, new CharacterDamagedEvent("Goblin", 25));
```

## Thread safety
All operations (Subscribe, Unsubscribe, Publish, Reset) are protected by a lock and safe for concurrent use.
During event dispatch, handlers are called using a snapshot copy, so you can safely modify subscriptions while publishing.

## API Overview
| Method                                                  | Description                                  |
| ------------------------------------------------------- | -------------------------------------------- |
| `IDisposable Subscribe<T>(EventHandler<T> handler)`     | Subscribe to event `T`.                      |
| `IDisposable SubscribeOnce<T>(EventHandler<T> handler)` | Subscribe for one-time delivery.             |
| `bool Unsubscribe<T>(EventHandler<T> handler)`          | Remove specific subscription.                |
| `void Publish<T>(object? sender, T eventArgs)`          | Publish event to all subscribers.            |
| `void Publish<T>(T eventArgs)`                          | Publish event to all subscribers.            |
| `int GetSubscriberCount<T>()`                           | Get count of current subscribers for a type. |
| `void Reset()`                                          | Remove all subscriptions.                    |

## Changes
 - v1.0 - Initial release.
 
## TrimKit Collection
This library is part of the **TrimKit** collection - a set of small, focused C# libraries that make game development more enjoyable by reducing the need for boilerplate code and providing simple reusable building blocks that can be dropped into any project.

- [TrimKit.EventBus](https://github.com/Lurler/TrimKit.EventBus) - Lightweight, mutation-safe event bus (event aggregator).
- [TrimKit.GameSettings](https://github.com/Lurler/TrimKit.GameSettings) - JSON-based persistent settings manager.
- [TrimKit.VirtualFileSystem](https://github.com/Lurler/TrimKit.VirtualFileSystem) - Unified file hierarchy abstraction to enable modding and additional content in games.

Each module is independent and can be used standalone or combined with others for a complete lightweight foundation.

## Contribution
Contributions are welcome!

You can start with submitting an [issue on GitHub](https://github.com/Lurler/TrimKit.EventBus/issues).

## License
This library is released under the [MIT License](../master/LICENSE).