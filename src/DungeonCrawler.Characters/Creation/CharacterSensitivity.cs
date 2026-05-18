using DungeonCrawler.Characters.Models;

namespace DungeonCrawler.Characters.Creation;

public enum CharacterSensitivity
{
    Insensitive, Low, Normal, High, Hypersensitive
}

public static class CharacterSensitivityExtensions
{
    public static AttributesModifier Modifier(this CharacterSensitivity s) => s switch
    {
        CharacterSensitivity.Insensitive => new(Musculature: +1, Brain: -2, Vitality: +2),
        CharacterSensitivity.Low => new(Brain: -1, Vitality: +1),
        CharacterSensitivity.Normal => AttributesModifier.Zero,
        CharacterSensitivity.High => new(Musculature: -1, Flexibility: -1, Brain: +1, Vitality: -1),
        CharacterSensitivity.Hypersensitive => new(Musculature: -2, Flexibility: -2, Brain: +2, Vitality: -2),
        _ => AttributesModifier.Zero
    };
}