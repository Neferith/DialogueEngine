namespace DungeonCrawler.EventSystems;

public enum EventTrigger
{
    GameStart,
    MapEnter,
    TurnPassed,
    TileEnter,
    Interact,
    Proximity
}

public class EventCondition
{
    public string? FlagNotSet { get; set; }
    public string? FlagSet { get; set; }
    public string? NpcAlive { get; set; }
    public string? NpcNotAlive { get; set; }
    public int? MinTurn { get; set; }
}

public class EventEffect
{
    public string ScriptId { get; set; } = "";
    public Dictionary<string, object> Params { get; set; } = new();
}

public class GameEvent
{
    public string Id { get; set; } = "";
    public EventTrigger Trigger { get; set; }
    public string? MapId { get; set; }  // null = global
    public int? TileX { get; set; }
    public int? TileY { get; set; }
    public string? EntityId { get; set; }
    public int? Radius { get; set; }

    public EventCondition Condition { get; set; } = new();
    public List<EventEffect> Effects { get; set; } = new();

    public bool IsGlobal => MapId == null;
}