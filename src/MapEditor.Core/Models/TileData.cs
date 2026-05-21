namespace MapEditor.Core.Models;

public class TileData
{
    public PositionData  Position   { get; set; } = new();
    public string        TileTypeId { get; set; } = "";
    public bool          Walkable   { get; set; }
    public MapTransition? Transition { get; set; }

    public List<TileItemData> Items { get; set; } = new();
}

public class TileItemData
{
    public string Id { get; set; } = "";
    public int Quantity { get; set; } = 1;
}
