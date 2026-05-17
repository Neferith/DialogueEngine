namespace DungeonCrawler.Core.Models;

/// <summary>
/// Chemins absolus des textures du biome courant.
/// Produit par DungeonCrawler.MapLoader à partir d'un ModuleDefinition.
/// </summary>
public record BiomeTextures(
    string? Wall,
    string? Floor,
    string? Ceiling,
    string? DoorClosed,
    string? DoorOpen);