namespace MapEditor.Core.Models;

/// <summary>Root document serialized to / from JSON.</summary>
public class MapFile
{
    public string   Id                { get; set; } = "";
    public string   MapType           { get; set; } = "DUNGEON";
    public string   ModuleId          { get; set; } = "";
    public SizeData Size              { get; set; } = new();
    public string   DefaultTileTypeId { get; set; } = "";

    /// <summary>Only tiles that differ from the default are stored (sparse).</summary>
    public List<TileData>       Tiles    { get; set; } = new();
    public List<EntityPlacement> Entities { get; set; } = new();
}
