using DungeonCrawler.Core.Entities.Behaviors;
using DungeonCrawler.Core.Models;

namespace DungeonCrawler.Core.Entities;

public class MonsterEntity : DungeonEntity
{
    public string         Name     { get; set; }
    public IEntityBehavior Behavior { get; set; }
    public override bool  BlocksMovement => true;

    public MonsterEntity(string id, GridPosition position, string name, IEntityBehavior? behavior = null)
        : base(id, position)
    {
        Name     = name;
        Behavior = behavior ?? new PassiveBehavior();
    }
}
