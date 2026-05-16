namespace DungeonCrawler.MapLoader;

/// <summary>
/// Permet à LoadedMap.EntitiesOfCategory() de connaître la catégorie d'un type d'entité
/// sans dépendre directement de ModuleDefinition.
/// Implémenté par ModuleEntityResolver fourni lors du chargement.
/// </summary>
public interface IEntityCategoryResolver
{
    string? GetCategory(string entityTypeId);
}
