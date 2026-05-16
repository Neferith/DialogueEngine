namespace MapEditor.Core.Models;

public class MapTransition
{
    public string TargetMapId       { get; set; } = "";
    public PositionData TargetPosition  { get; set; } = new();
    public string TargetOrientation { get; set; } = "NORTH";
}
