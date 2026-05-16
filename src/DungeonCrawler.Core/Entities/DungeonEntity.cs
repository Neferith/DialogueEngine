using DungeonCrawler.Core.Models;

namespace DungeonCrawler.Core.Entities;

/// <summary>
/// Base class for anything that lives on the dungeon grid:
/// monsters, NPCs, items on the floor.
/// </summary>
public abstract class DungeonEntity
{
    public string       Id       { get; }
    public GridPosition Position { get; set; }
    public bool         IsActive { get; set; } = true;

    /// <summary>True if the entity occupies its tile and prevents movement into it.</summary>
    public abstract bool BlocksMovement { get; }

    protected DungeonEntity(string id, GridPosition position)
    {
        Id       = id;
        Position = position;
    }
}
