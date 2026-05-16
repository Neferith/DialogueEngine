using DungeonCrawler.Core.Models;

namespace DungeonCrawler.Core.Entities;

/// <summary>
/// An item lying on the dungeon floor. Does not block movement.
/// Picked up via the Interact action.
/// </summary>
public class ItemEntity : DungeonEntity
{
    public string ItemId      { get; set; }
    public string DisplayName { get; set; }
    public override bool BlocksMovement => false;

    public ItemEntity(string id, GridPosition position, string itemId, string displayName)
        : base(id, position)
    {
        ItemId      = itemId;
        DisplayName = displayName;
    }
}
