using DungeonCrawler.Characters.Models;

namespace DungeonCrawler.Characters.Creation;

public enum CharacterSize
{
    Dwarf, Small, Medium, Tall, Large, Giant
}

public static class CharacterSizeExtensions
{
    public static (int MinCm, int MaxCm) HeightRange(this CharacterSize size) => size switch
    {
        CharacterSize.Dwarf => (130, 150),
        CharacterSize.Small => (150, 165),
        CharacterSize.Medium => (165, 185),
        CharacterSize.Tall => (185, 200),
        CharacterSize.Large => (200, 220),
        CharacterSize.Giant => (220, 250),
        _ => (0, 0)
    };

    public static AttributesModifier Modifier(this CharacterSize size) => size switch
    {
        CharacterSize.Dwarf => new(Musculature: +1, Flexibility: -2, Vitality: +1),
        CharacterSize.Small => new(Musculature: -2, Flexibility: +2, Vitality: +1),
        CharacterSize.Medium => new(Musculature: -1, Flexibility: +1),
        CharacterSize.Tall => new(Musculature: +1, Flexibility: -1),
        CharacterSize.Large => new(Musculature: +1, Flexibility: -2, Vitality: +1),
        CharacterSize.Giant => new(Musculature: +2, Flexibility: -4, Vitality: +2),
        _ => AttributesModifier.Zero
    };

    public static IReadOnlyList<CharacterWeight> AvailableWeights(this CharacterSize size) =>
        size switch
        {
            CharacterSize.Dwarf => [CharacterWeight.Light, CharacterWeight.Average],
            CharacterSize.Small => [CharacterWeight.Light, CharacterWeight.Average, CharacterWeight.Heavy],
            CharacterSize.Medium => [CharacterWeight.Light, CharacterWeight.Average, CharacterWeight.Heavy, CharacterWeight.VeryHeavy],
            CharacterSize.Tall => [CharacterWeight.Average, CharacterWeight.Heavy, CharacterWeight.VeryHeavy],
            CharacterSize.Large => [CharacterWeight.Heavy, CharacterWeight.VeryHeavy, CharacterWeight.Extreme],
            CharacterSize.Giant => [CharacterWeight.VeryHeavy, CharacterWeight.Extreme],
            _ => []
        };
}