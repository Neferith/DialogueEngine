using DungeonCrawler.Core.Characters;
using DungeonCrawler.Core.Models;
using DungeonCrawler.Core.Systems;

namespace DungeonCrawler.Core.Entities.Behaviors;

/// <summary>Stands still. Use for statues, sleeping guards, decorative monsters.</summary>
public class PassiveBehavior : IEntityBehavior
{
    public EntityAction Act(MonsterEntity monster, EntitySystem entitySystem, DungeonMap map, Party party)
        => new EntityWaited(monster);
}
