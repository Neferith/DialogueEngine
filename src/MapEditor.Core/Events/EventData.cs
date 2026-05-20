namespace MapEditor.Core.Events;

public class EventFile
{
    public List<EventData> Events { get; set; } = new();
}

public class EventData
{
    public string Id { get; set; } = "";
    public string Trigger { get; set; } = "";   // MapEnter, TileEnter, etc.
    public string? MapId { get; set; }
    public int? TileX { get; set; }
    public int? TileY { get; set; }
    public string? EntityId { get; set; }
    public int? Radius { get; set; }

    public EventConditionData Condition { get; set; } = new();
    public List<EventEffectData> Effects { get; set; } = new();
}

public class EventConditionData
{
    public string? FlagNotSet { get; set; }
    public string? FlagSet { get; set; }
    public string? NpcAlive { get; set; }
    public string? NpcNotAlive { get; set; }
    public int? MinTurn { get; set; }
}

public class EventEffectData
{
    public string ScriptId { get; set; } = "";
    public Dictionary<string, string> Params { get; set; } = new();
}