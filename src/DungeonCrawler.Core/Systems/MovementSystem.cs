using DungeonCrawler.Core.Models;
using DungeonCrawler.Core.Characters;

namespace DungeonCrawler.Core.Systems;

public class MovementSystem
{
    private readonly DungeonMap _map;

    public MovementSystem(DungeonMap map) => _map = map;

    // ── Translations ──────────────────────────────────────────────────────────

    public MoveResult MoveForward(Party party)  => TryMove(party, party.Facing);
    public MoveResult MoveBackward(Party party) => TryMove(party, party.Facing.Opposite());
    public MoveResult StrafeLeft(Party party)   => TryMove(party, party.Facing.TurnLeft());
    public MoveResult StrafeRight(Party party)  => TryMove(party, party.Facing.TurnRight());

    // ── Rotations (never blocked) ─────────────────────────────────────────────

    public void TurnLeft(Party party)  => party.Facing = party.Facing.TurnLeft();
    public void TurnRight(Party party) => party.Facing = party.Facing.TurnRight();
    public void Turn180(Party party)   => party.Facing = party.Facing.Opposite();

    // ── Internal ──────────────────────────────────────────────────────────────

    private MoveResult TryMove(Party party, Direction direction)
    {
        var target = party.Position + direction.ToOffset();

        if (!_map.IsInBounds(target)) return MoveResult.OutOfBounds;
        if (_map.IsSolid(target))     return MoveResult.BlockedByWall;

        party.Position = target;
        return MoveResult.Success;
    }
}
