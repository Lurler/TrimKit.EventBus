namespace EventBusTest;

public class CharacterDamagedEvent : EventArgs
{
    public string Name { get; }
    public int Damage { get; }

    public CharacterDamagedEvent(string playerName, int damage)
    {
        Name = playerName;
        Damage = damage;
    }
}