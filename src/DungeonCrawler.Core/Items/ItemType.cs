namespace DungeonCrawler.Core;

public abstract record ItemType
{
    public record Other() : ItemType;
    public record Quest() : ItemType;
    public record Equipment() : ItemType;
    // Futur : Consumable, Material
}