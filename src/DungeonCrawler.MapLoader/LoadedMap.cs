using DungeonCrawler.Core.Models;
using MapEditor.Core.Models;

namespace DungeonCrawler.MapLoader;

/// <summary>
/// Résultat du chargement d'un MapFile vers le moteur de jeu.
/// Les entités restent sous forme brute (EntityPlacement) — 
/// c'est la couche jeu qui les instancie en NpcEntity, ItemEntity, etc.
/// </summary>
public class LoadedMap
{
    /// <summary>La map prête à être passée à DungeonRunner.</summary>
    public DungeonMap Map { get; }

    /// <summary>
    /// Transitions entre maps, indexées par position.
    /// Quand la party entre sur une de ces cases, charger la map cible.
    /// </summary>
    public IReadOnlyDictionary<GridPosition, MapTransition> Transitions { get; }

    /// <summary>Entités brutes telles que définies dans l'éditeur.</summary>
    public IReadOnlyList<EntityPlacement> Entities { get; }

    /// <summary>Position de départ de la party (depuis l'entité PLAYER_SPAWN).</summary>
    public GridPosition? PlayerSpawn { get; }

    /// <summary>Orientation de départ (depuis l'entité PLAYER_SPAWN).</summary>
    public Direction PlayerFacing { get; }

    /// <summary>ID du module utilisé pour construire cette map.</summary>
    public string ModuleId { get; }

    public LoadedMap(
        DungeonMap map,
        IReadOnlyDictionary<GridPosition, MapTransition> transitions,
        IReadOnlyList<EntityPlacement> entities,
        GridPosition? playerSpawn,
        Direction playerFacing,
        string moduleId)
    {
        Map          = map;
        Transitions  = transitions;
        Entities     = entities;
        PlayerSpawn  = playerSpawn;
        PlayerFacing = playerFacing;
        ModuleId     = moduleId;
    }

    /// <summary>Retourne la transition à la position donnée, ou null.</summary>
    public MapTransition? GetTransitionAt(GridPosition pos) =>
        Transitions.TryGetValue(pos, out var t) ? t : null;

    /// <summary>
    /// Entités d'une catégorie donnée ("NPC", "ITEM", "SPAWN").
    /// </summary>
    public IEnumerable<EntityPlacement> EntitiesOfCategory(string category, IEntityCategoryResolver resolver) =>
        Entities.Where(e => resolver.GetCategory(e.EntityTypeId) == category);
}
