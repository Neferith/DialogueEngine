using DungeonCrawler.Characters.Models;

namespace DungeonCrawler.Characters.Creation;

public enum CharacterGender
{
    Male,
    Female
}

public static class CharacterGenderExtensions
{
    public static AttributesModifier Modifier(this CharacterGender gender) => gender switch
    {
        CharacterGender.Male => new(Musculature: +1, Vitality: +1),
        CharacterGender.Female => new(Flexibility: +2),
        _ => AttributesModifier.Zero
    };

    public static IReadOnlyList<CharacterSize> AvailableSizes(this CharacterGender gender) =>
        gender switch
        {
            CharacterGender.Male => Enum.GetValues<CharacterSize>().ToArray(),
            CharacterGender.Female => [
                CharacterSize.Dwarf, CharacterSize.Small,
                CharacterSize.Medium, CharacterSize.Tall
            ],
            _ => []
        };
}