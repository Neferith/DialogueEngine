namespace MapEditor.Core.Models;

/// <summary>
/// Résumé léger d'une map pour le navigateur — pas besoin de charger tout le MapFile.
/// </summary>
public class MapSummary
{
    public string Id { get; init; } = "";
    public string FilePath { get; init; } = "";
    public string ModuleId { get; init; } = "";
    public string MapType { get; init; } = "";
    public int Width { get; init; }
    public int Height { get; init; }

    public string DisplayName => $"{Id}  ({Width}×{Height})";

    public static MapSummary From(MapFile map, string filePath) => new()
    {
        Id = map.Id,
        FilePath = filePath,
        ModuleId = map.ModuleId,
        MapType = map.MapType,
        Width = map.Size.Width,
        Height = map.Size.Height
    };
}