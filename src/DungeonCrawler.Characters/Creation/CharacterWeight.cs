using DungeonCrawler.Characters.Models;

namespace DungeonCrawler.Characters.Creation;

public enum CharacterWeight
{
    Light, Average, Heavy, VeryHeavy, Extreme
}

public static class CharacterWeightExtensions
{
    public static (int MinKg, int MaxKg) WeightRange(this CharacterWeight w) => w switch
    {
        CharacterWeight.Light => (40, 60),
        CharacterWeight.Average => (60, 85),
        CharacterWeight.Heavy => (85, 110),
        CharacterWeight.VeryHeavy => (110, 140),
        CharacterWeight.Extreme => (140, 200),
        _ => (0, 0)
    };

    public static AttributesModifier Modifier(this CharacterWeight w) => w switch
    {
        CharacterWeight.Light => new(Musculature: -2, Flexibility: +2, Vitality: -1),
        CharacterWeight.Average => AttributesModifier.Zero,
        CharacterWeight.Heavy => new(Musculature: +2, Flexibility: -1, Vitality: +1),
        CharacterWeight.VeryHeavy => new(Musculature: +4, Flexibility: -2, Vitality: +2),
        CharacterWeight.Extreme => new(Musculature: +6, Flexibility: -3, Vitality: +3),
        _ => AttributesModifier.Zero
    };

    public static IReadOnlyList<CharacterSensitivity> AvailableSensitivities(this CharacterWeight w) =>
        w switch
        {
            CharacterWeight.Light => [CharacterSensitivity.Hypersensitive, CharacterSensitivity.High],
            CharacterWeight.Average => [CharacterSensitivity.High, CharacterSensitivity.Normal, CharacterSensitivity.Low],
            CharacterWeight.Heavy => [CharacterSensitivity.High, CharacterSensitivity.Normal, CharacterSensitivity.Low],
            CharacterWeight.VeryHeavy => [CharacterSensitivity.Normal, CharacterSensitivity.Low],
            CharacterWeight.Extreme => [CharacterSensitivity.Low, CharacterSensitivity.Insensitive],
            _ => []
        };
}