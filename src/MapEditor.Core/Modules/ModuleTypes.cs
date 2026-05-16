namespace MapEditor.Core.Modules;

public class PropertyDefinition
{
    public string Key     { get; set; } = "";
    /// <summary>"string" | "int" | "float" | "bool"</summary>
    public string Type    { get; set; } = "string";
    public string Default { get; set; } = "";
}

public class TileTypeDefinition
{
    public string  Id              { get; set; } = "";
    public string  Name            { get; set; } = "";
    public bool    DefaultWalkable { get; set; } = true;
    public bool    IsWall          { get; set; }
    public int     SpriteIndex     { get; set; }
    /// <summary>Hex color used when no texture is loaded, e.g. "#404040".</summary>
    public string? Color           { get; set; }

    // ── Champs optionnels pour DungeonCrawler.MapLoader ───────────────────────
    // Stockés en string pour que MapEditor.Core ne dépende pas de DungeonCrawler.Core.

    /// <summary>"None" | "Door" | "StairsUp" | "StairsDown" | "Trigger" | "LevelExit"</summary>
    public string? TileTag     { get; set; }
    /// <summary>"Stone" | "Dirt" | "Water" | "Lava"</summary>
    public string? FloorType   { get; set; }
    /// <summary>"Stone" | "Open"</summary>
    public string? CeilingType { get; set; }
}

public class EntityTypeDefinition
{
    public string  Id           { get; set; } = "";
    public string  Name         { get; set; } = "";
    /// <summary>"SPAWN" | "NPC" | "ITEM"</summary>
    public string  Category     { get; set; } = "NPC";
    public string? Faction      { get; set; }
    public int     SpriteIndex  { get; set; }
    public string? Color        { get; set; }
    public List<PropertyDefinition> Properties { get; set; } = new();
}
