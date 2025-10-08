namespace EventBusTest;

public class GameStartedEvent : EventArgs
{
    public DateTime StartTime { get; }

    public GameStartedEvent(DateTime startTime)
    {
        StartTime = startTime;
    }
}
