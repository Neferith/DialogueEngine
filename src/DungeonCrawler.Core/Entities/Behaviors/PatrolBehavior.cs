using DungeonCrawler.Core.Characters;
using DungeonCrawler.Core.Models;
using DungeonCrawler.Core.Systems;

namespace DungeonCrawler.Core.Entities.Behaviors;

/// <summary>
/// Walks back and forth along a fixed axis until blocked, then reverses.
/// Simple, deterministic, cheap. Good for guards and wandering creatures.
/// </summary>
public class PatrolBehavior : IEntityBehavior
{
    private readonly Direction _axis;
    private bool _movingForward = true;

    /// <param name="axis">The direction to start patrolling toward.</param>
    public PatrolBehavior(Direction axis) => _axis = axis;

    public EntityAction Act(MonsterEntity monster, EntitySystem entitySystem, DungeonMap map, Party party)
    {
        var direction = _movingForward ? _axis : _axis.Opposite();
        var target    = monster.Position + direction.ToOffset();

        if (CanMoveTo(target, map, entitySystem))
        {
            var from = monster.Position;
            entitySystem.MoveEntity(monster, target);
            return new EntityMoved(monster, from, target);
        }

        // Hit an obstacle — try the other direction immediately this turn
        _movingForward = !_movingForward;
        direction      = _movingForward ? _axis : _axis.Opposite();
        target         = monster.Position + direction.ToOffset();

        if (CanMoveTo(target, map, entitySystem))
        {
            var from = monster.Position;
            entitySystem.MoveEntity(monster, target);
            return new EntityMoved(monster, from, target);
        }

        // Completely stuck (trapped)
        return new EntityWaited(monster);
    }

    private static bool CanMoveTo(GridPosition target, DungeonMap map, EntitySystem entities)
        => map.IsPassable(target) && !entities.HasBlockingEntity(target);
}
