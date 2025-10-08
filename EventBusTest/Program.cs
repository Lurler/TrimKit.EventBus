using TrimKit.EventBus;

namespace EventBusTest;

internal class Program
{
    static void Main(string[] args)
    {
        Console.WriteLine("=== Simple.EventBus Demo ===");

        var bus = new EventBus();
        Console.WriteLine("Created event bus.");

        // subscribe normally
        var damageToken = bus.Subscribe<CharacterDamagedEvent>(OnPlayerDamaged);
        

        // subscribe once using lambda
        bus.SubscribeOnce<GameStartedEvent>((s, e) =>
        {
            Console.WriteLine($"[Once] Game started at {e.StartTime:T}");
        });
        Console.WriteLine("Subscribed to events.");
        Console.WriteLine();


        Console.WriteLine("Start publishing events.");

        // publish first event
        bus.Publish(new GameStartedEvent(DateTime.Now));

        // publish again (once-subscriber will not be called again)
        bus.Publish(new GameStartedEvent(DateTime.Now));

        // publish CharacterDamagedEvent event a few times
        bus.Publish(null, new CharacterDamagedEvent("Goblin", 25));
        bus.Publish(null, new CharacterDamagedEvent("Orc", 10));
        bus.Publish(null, new CharacterDamagedEvent("Dragon", 999));

        Console.WriteLine();
        Console.WriteLine("Checking unsubscribe now.");

        // check current number of subscribers to CharacterDamagedEvent
        Console.WriteLine($"Subscribers for CharacterDamagedEvent: {bus.GetSubscriberCount<CharacterDamagedEvent>()}");

        // unsubscribe
        damageToken.Dispose();

        // unsubscribing again, it should not produce any errors
        damageToken.Dispose();

        Console.WriteLine($"Subscribers for CharacterDamagedEvent after unsubscribe: {bus.GetSubscriberCount<CharacterDamagedEvent>()}");

        // try publishing again (no one will handle it)
        bus.Publish(null, new CharacterDamagedEvent("Puppy", 50));

        Console.WriteLine();

        // reset bus
        bus.Reset();
        Console.WriteLine("EventBus reset.");

        Console.WriteLine("Test end.");

        Console.ReadLine();
    }

    private static void OnPlayerDamaged(object? sender, CharacterDamagedEvent e)
    {
        Console.WriteLine($"Character {e.Name} took {e.Damage} damage!");
    }
}
