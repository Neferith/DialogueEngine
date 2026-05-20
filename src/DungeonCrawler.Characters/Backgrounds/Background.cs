using DungeonCrawler.Characters.Models;
using DungeonCrawler.Characters.Creation;

namespace DungeonCrawler.Characters.Backgrounds;

public class BackgroundRequirement
{
    public List<CharacterSize>? AllowedSizes { get; set; }

    public bool IsMet(CharacterSize size) =>
        AllowedSizes == null || AllowedSizes.Contains(size);
}

public class Background
{
    public string Id { get; set; } = "";
    public string Name { get; set; } = "";
    public string Description { get; set; } = "";
    public string TypeId { get; set; } = "";
    public AttributesModifier AttributesModifier { get; set; } = AttributesModifier.Zero;
    public List<string> StartingSkillIds { get; set; } = new();
    public BackgroundRequirement? Requirement { get; set; }
}

public class BackgroundType
{
    public string Id { get; set; } = "";
    public string Name { get; set; } = "";
    public string Description { get; set; } = "";
    public List<Background> Backgrounds { get; set; } = new();
}