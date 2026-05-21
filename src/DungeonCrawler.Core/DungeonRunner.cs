using DungeonCrawler.Core.Characters;
using DungeonCrawler.Core.Models;
using DungeonCrawler.Core.Rendering;
using DungeonCrawler.Core.Systems;

namespace DungeonCrawler.Core;

/// <summary>
/// Main entry point for the dungeon engine.
/// Owns the map, the party, and the core systems.
/// Use TurnManager for sequenced gameplay; DungeonRunner for direct access.
/// </summary>
public class DungeonRunner
{
    public DungeonMap Map { get; }
    public Party Party { get; }

    private readonly MovementSystem _movement;
    private readonly ViewBuilder _viewBuilder;

    public DungeonRunner(DungeonMap map, Party party, ViewConfig? viewConfig = null)
    {
        Map = map;
        Party = party;
        _movement = new MovementSystem(map);
        _viewBuilder = new ViewBuilder(viewConfig);
    }

    // ── View ──────────────────────────────────────────────────────────────────

    public DungeonView GetView() => _viewBuilder.Build(Party, Map);

    // ── Movement ──────────────────────────────────────────────────────────────

    public MoveResult MoveForward() => _movement.MoveForward(Party);
    public MoveResult MoveBackward() => _movement.MoveBackward(Party);
    public MoveResult StrafeLeft() => _movement.StrafeLeft(Party);
    public MoveResult StrafeRight() => _movement.StrafeRight(Party);

    public void TurnLeft() => _movement.TurnLeft(Party);
    public void TurnRight() => _movement.TurnRight(Party);
    public void Turn180() => _movement.Turn180(Party);

    internal MoveResult MoveInDirection(Direction direction)
    {
        var facing = Party.Facing;
        if (direction == facing) return MoveForward();
        if (direction == facing.Opposite()) return MoveBackward();
        if (direction == facing.TurnLeft()) return StrafeLeft();
        return StrafeRight();
    }
}