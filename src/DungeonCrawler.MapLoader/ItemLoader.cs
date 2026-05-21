using MapEditor.Core.Items;
using DungeonCrawler.Core;

namespace DungeonCrawler.MapLoader;

public class ItemLoader
{
    public ItemRegistry Load(string path)
    {
        var registry = new ItemRegistry();
        var file = ItemsSerializer.Load(path);

        foreach (var data in file.Items)
        {
            var item = new ItemDefinition
            {
                Id = data.Id,
                Title = data.Title,
                Description = data.Description,
                Type = ParseType(data.Type),
                StackRules = new StackRules(data.Stackable, data.MaxStack),
                SpritePath = data.SpritePath
            };
            registry.Register(item);
        }

        Console.WriteLine($"[ItemLoader] {file.Items.Count} item(s) chargé(s).");
        return registry;
    }

    private static ItemType ParseType(string type) =>
        type.ToLowerInvariant() switch
        {
            "quest" => new ItemType.Quest(),
            "equipment" => new ItemType.Equipment(),
            _ => new ItemType.Other()
        };
}