using DungeonCrawler.Core.Characters;
using DungeonCrawler.Core.Models;
using DungeonCrawler.Core.Systems;

namespace DungeonCrawler.Core.Entities.Behaviors;

/// <summary>
/// Decides what a monster does on its turn.
/// Implement this to add new AI patterns without touching MonsterEntity.
/// </summary>
public interface IEntityBehavior
{
    EntityAction Act(
        MonsterEntity monster,
        EntitySystem  entitySystem,
        DungeonMap    map,
        Party         party);
}
