namespace MapEditor.Core.Models;

public class TileData
{
    public PositionData  Position   { get; set; } = new();
    public string        TileTypeId { get; set; } = "";
    public bool          Walkable   { get; set; }
    public MapTransition? Transition { get; set; }
}
