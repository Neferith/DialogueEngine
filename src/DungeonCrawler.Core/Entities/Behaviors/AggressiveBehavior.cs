using DungeonCrawler.Core.Characters;
using DungeonCrawler.Core.Models;
using DungeonCrawler.Core.Systems;

namespace DungeonCrawler.Core.Entities.Behaviors;

/// <summary>
/// Idles until the party enters detection range, then chases greedily.
/// Adjacent to party → attacks. No pathfinding yet; a* can replace the
/// greedy step later without touching anything else.
/// </summary>
public class AggressiveBehavior : IEntityBehavior
{
    public int DetectionRange { get; }

    public AggressiveBehavior(int detectionRange = 5) => DetectionRange = detectionRange;

    public EntityAction Act(MonsterEntity monster, EntitySystem entitySystem, DungeonMap map, Party party)
    {
        int dist = ManhattanDistance(monster.Position, party.Position);

        if (dist > DetectionRange)
            return new EntityWaited(monster);

        if (dist <= 1)
            return new EntityAttacked(monster, party.Position);

        // Greedy step: move along the axis with the greatest remaining distance
        var direction = GreedyStepToward(monster.Position, party.Position);
        var target    = monster.Position + direction.ToOffset();

        if (map.IsPassable(target) && !entitySystem.HasBlockingEntity(target))
        {
            var from = monster.Position;
            entitySystem.MoveEntity(monster, target);
            return new EntityMoved(monster, from, target);
        }

        // Primary axis blocked — try the perpendicular axis
        var alt    = PerpendicularStepToward(monster.Position, party.Position, direction);
        var altPos = monster.Position + alt.ToOffset();

        if (map.IsPassable(altPos) && !entitySystem.HasBlockingEntity(altPos))
        {
            var from = monster.Position;
            entitySystem.MoveEntity(monster, altPos);
            return new EntityMoved(monster, from, altPos);
        }

        return new EntityWaited(monster);
    }

    // ── Helpers ───────────────────────────────────────────────────────────────

    private static int ManhattanDistance(GridPosition a, GridPosition b)
        => Math.Abs(a.X - b.X) + Math.Abs(a.Y - b.Y);

    /// <summary>Pick the cardinal direction that most reduces distance to target.</summary>
    private static Direction GreedyStepToward(GridPosition from, GridPosition to)
    {
        int dx = to.X - from.X;
        int dy = to.Y - from.Y;

        return Math.Abs(dx) >= Math.Abs(dy)
            ? (dx > 0 ? Direction.East  : Direction.West)
            : (dy > 0 ? Direction.North : Direction.South);
    }

    /// <summary>Perpendicular fallback when the primary axis is blocked.</summary>
    private static Direction PerpendicularStepToward(GridPosition from, GridPosition to,
                                                      Direction primary)
    {
        int dx = to.X - from.X;
        int dy = to.Y - from.Y;

        // If primary was horizontal, try vertical, and vice-versa
        return primary is Direction.East or Direction.West
            ? (dy > 0 ? Direction.North : Direction.South)
            : (dx > 0 ? Direction.East  : Direction.West);
    }
}
