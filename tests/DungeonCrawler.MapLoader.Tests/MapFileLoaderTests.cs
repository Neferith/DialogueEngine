using DungeonCrawler.Core.Models;
using DungeonCrawler.MapLoader;
using FluentAssertions;
using MapEditor.Core.Models;
using Xunit;

namespace DungeonCrawler.MapLoader.Tests;

public class MapFileLoaderTests
{
    private static readonly string FixturesPath =
        Path.Combine(AppContext.BaseDirectory, "fixtures");

    private readonly MapFileLoader _loader = new();

    // ── Helpers ───────────────────────────────────────────────────────────────

    private LoadedMap Load(MapFile map) => _loader.Convert(map, FixturesPath);

    private static MapFile MakeMap(int width = 8, int height = 8,
                                   string defaultTile = "STONE_WALL") => new()
                                   {
                                       Id = "test_map",
                                       ModuleId = "stone_dungeon",
                                       Size = new SizeData(width, height),
                                       DefaultTileTypeId = defaultTile
                                   };

    // ── Dimensions ────────────────────────────────────────────────────────────

    [Fact]
    public void Map_HasCorrectDimensions()
    {
        var loaded = Load(MakeMap(10, 6));

        loaded.Map.Width.Should().Be(10);
        loaded.Map.Height.Should().Be(6);
    }

    // ── Tile par défaut ───────────────────────────────────────────────────────

    [Fact]
    public void DefaultTile_FillsEntireMap()
    {
        var loaded = Load(MakeMap(defaultTile: "STONE_WALL"));

        for (int x = 0; x < loaded.Map.Width; x++)
            for (int y = 0; y < loaded.Map.Height; y++)
                loaded.Map.GetTile(x, y)!.IsSolid.Should().BeTrue(
                    because: $"STONE_WALL est solide ({x},{y})");
    }

    [Fact]
    public void DefaultFloorTile_IsNotSolid()
    {
        var loaded = Load(MakeMap(defaultTile: "STONE_FLOOR"));

        loaded.Map.GetTile(0, 0)!.IsSolid.Should().BeFalse();
    }

    // ── Override de tiles ─────────────────────────────────────────────────────

    [Fact]
    public void TileOverride_AppliesCorrectly()
    {
        var map = MakeMap(defaultTile: "STONE_WALL");
        map.Tiles.Add(new TileData
        {
            Position = new PositionData(3, 4),
            TileTypeId = "STONE_FLOOR",
            Walkable = true
        });

        var loaded = Load(map);

        loaded.Map.GetTile(3, 4)!.IsSolid.Should().BeFalse();
        // Les autres restent des murs
        loaded.Map.GetTile(0, 0)!.IsSolid.Should().BeTrue();
    }

    [Fact]
    public void DoorTile_HasCorrectTag()
    {
        var map = MakeMap(defaultTile: "STONE_FLOOR");
        map.Tiles.Add(new TileData
        {
            Position = new PositionData(2, 2),
            TileTypeId = "STONE_DOOR",
            Walkable = false
        });

        var loaded = Load(map);

        loaded.Map.GetTile(2, 2)!.Tag.Should().Be(TileTag.Door);
    }

    // ── Player spawn ──────────────────────────────────────────────────────────

    [Fact]
    public void PlayerSpawn_IsNullWhenNoEntity()
    {
        var loaded = Load(MakeMap());

        loaded.PlayerSpawn.Should().BeNull();
    }

    [Fact]
    public void PlayerSpawn_IsReadFromEntity()
    {
        var map = MakeMap(defaultTile: "STONE_FLOOR");
        map.Entities.Add(new EntityPlacement
        {
            Id = "spawn",
            EntityTypeId = "PLAYER_SPAWN",
            Position = new PositionData(2, 5),
            Orientation = "EAST"
        });

        var loaded = Load(map);

        loaded.PlayerSpawn.Should().Be(new GridPosition(2, 5));
        loaded.PlayerFacing.Should().Be(Direction.East);
    }

    // ── Transitions ───────────────────────────────────────────────────────────

    [Fact]
    public void Transition_IsStoredAtCorrectPosition()
    {
        var map = MakeMap(defaultTile: "STONE_FLOOR");
        map.Tiles.Add(new TileData
        {
            Position = new PositionData(7, 0),
            TileTypeId = "STONE_FLOOR",
            Walkable = true,
            Transition = new MapTransition
            {
                TargetMapId = "dungeon_02",
                TargetPosition = new PositionData(1, 8),
                TargetOrientation = "NORTH"
            }
        });

        var loaded = Load(map);

        var transition = loaded.GetTransitionAt(new GridPosition(7, 0));
        transition.Should().NotBeNull();
        transition!.TargetMapId.Should().Be("dungeon_02");
        transition.TargetPosition.X.Should().Be(1);
        transition.TargetPosition.Y.Should().Be(8);
    }

    [Fact]
    public void Transition_IsNullForNormalTile()
    {
        var loaded = Load(MakeMap(defaultTile: "STONE_FLOOR"));

        loaded.GetTransitionAt(new GridPosition(3, 3)).Should().BeNull();
    }
}