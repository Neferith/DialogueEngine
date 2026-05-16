using DungeonCrawler.Core.Models;
using MapEditor.Core.Modules;

namespace DungeonCrawler.MapLoader;

/// <summary>
/// Convertit une TileTypeDefinition (monde éditeur) en Tile (monde moteur).
/// Les champs tileTag / floorType / ceilingType sont des strings dans le module.json
/// pour que MapEditor.Core ne dépende pas de DungeonCrawler.Core.
/// </summary>
public static class TileConverter
{
    public static Tile Convert(TileTypeDefinition def, bool? walkableOverride = null)
    {
        var isSolid = walkableOverride.HasValue
            ? !walkableOverride.Value
            : !def.DefaultWalkable;

        var tag        = ParseEnum<TileTag>    (def.TileTag,     TileTag.None);
        var floorType  = ParseEnum<FloorType>  (def.FloorType,   FloorType.Stone);
        var ceilType   = ParseEnum<CeilingType>(def.CeilingType, CeilingType.Stone);

        return new Tile
        {
            IsSolid   = isSolid,
            Tag       = tag,
            Floor     = floorType,
            Ceiling   = ceilType,
            TextureId = def.SpriteIndex
        };
    }

    private static T ParseEnum<T>(string? value, T fallback) where T : struct, Enum =>
        !string.IsNullOrWhiteSpace(value) && Enum.TryParse<T>(value, ignoreCase: true, out var result)
            ? result
            : fallback;
}
