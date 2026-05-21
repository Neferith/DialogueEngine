namespace DungeonCrawler.Core;

public record StackRules(bool Stackable = false, int Max = 1);

public class ItemDefinition
{
    public string Id { get; set; } = "";
    public string Title { get; set; } = "";
    public string Description { get; set; } = "";
    public ItemType Type { get; set; } = new ItemType.Other();
    public StackRules StackRules { get; set; } = new();
    public string SpritePath { get; set; } = "";
}