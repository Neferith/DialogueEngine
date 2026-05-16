using DungeonCrawler.Core.Models;

namespace DungeonCrawler.Core.Entities;

/// <summary>
/// Owns all entities in the current dungeon level.
/// Provides spatial queries used by behaviors and the renderer.
/// </summary>
public class EntitySystem
{
    private readonly Dictionary<string, DungeonEntity> _entities = new();

    // ── Registration ──────────────────────────────────────────────────────────

    public void Add(DungeonEntity entity)    => _entities[entity.Id] = entity;
    public void Remove(string id)
    {
        if (_entities.TryGetValue(id, out var e)) e.IsActive = false;
    }

    // ── Enumeration ───────────────────────────────────────────────────────────

    public IEnumerable<DungeonEntity> All         => _entities.Values.Where(e => e.IsActive);
    public IEnumerable<T>             GetAll<T>() where T : DungeonEntity => All.OfType<T>();

    // ── Spatial queries ───────────────────────────────────────────────────────

    public IEnumerable<DungeonEntity> GetAt(GridPosition pos)
        => All.Where(e => e.Position == pos);

    public T? GetAt<T>(GridPosition pos) where T : DungeonEntity
        => All.OfType<T>().FirstOrDefault(e => e.Position == pos);

    /// <summary>True if any active entity on this tile blocks movement.</summary>
    public bool HasBlockingEntity(GridPosition pos)
        => All.Any(e => e.Position == pos && e.BlocksMovement);

    // ── Mutations (called by behaviors) ───────────────────────────────────────

    public void MoveEntity(DungeonEntity entity, GridPosition newPosition)
        => entity.Position = newPosition;
}
