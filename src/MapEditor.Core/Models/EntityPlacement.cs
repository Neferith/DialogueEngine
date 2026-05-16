namespace MapEditor.Core.Models;

public class EntityPlacement
{
    public string       Id           { get; set; } = "";
    public string       EntityTypeId { get; set; } = "";
    public PositionData Position     { get; set; } = new();
    public string       Orientation  { get; set; } = "NORTH";

    /// <summary>Dynamic properties defined by the EntityTypeDefinition.</summary>
    public Dictionary<string, string> Properties { get; set; } = new();
}
