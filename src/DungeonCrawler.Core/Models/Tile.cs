namespace DungeonCrawler.Core.Models;

public enum TileTag    { None, Door, DoorOpen, StairsUp, StairsDown, Trigger, LevelExit }
public enum FloorType  { Stone, Dirt, Water, Lava }
public enum CeilingType { Stone, Open }

public class Tile
{
    /// <summary>Solid tiles (walls, closed doors) block movement and line of sight.</summary>
    public bool        IsSolid    { get; set; }

    public TileTag     Tag           { get; set; } = TileTag.None;
    public FloorType   FloorSurface  { get; set; } = FloorType.Stone;
    public CeilingType CeilingSurface { get; set; } = CeilingType.Stone;

    /// <summary>
    /// Texture / visual identifier. Interpretation is renderer-specific.
    /// 0 = default floor, 1 = default wall, etc.
    /// </summary>
    public int TextureId { get; set; }

    // ── Factory helpers ────────────────────────────────────────────────────────

    public static Tile Wall(int textureId = 1)    => new() { IsSolid = true,  TextureId = textureId };
    public static Tile Floor(int textureId = 0)   => new() { IsSolid = false, TextureId = textureId };
    public static Tile Door(int textureId = 2)    => new() { IsSolid = true,  Tag = TileTag.Door,       TextureId = textureId };
    public static Tile StairsUp(int textureId = 3)   => new() { IsSolid = false, Tag = TileTag.StairsUp,   TextureId = textureId };
    public static Tile StairsDown(int textureId = 4) => new() { IsSolid = false, Tag = TileTag.StairsDown, TextureId = textureId };

    public static Tile DoorOpen(int textureId = 5) => new() { IsSolid = false, Tag = TileTag.DoorOpen, TextureId = textureId };
}
