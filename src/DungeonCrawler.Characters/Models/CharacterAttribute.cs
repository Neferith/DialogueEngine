namespace DungeonCrawler.Characters.Models;

/// <summary>Un attribut d'un personnage. Plage : -10 à +10.</summary>
public record CharacterAttribute(int Permanent, int Min = -10, int Max = 10)
{
    public int Current() => Math.Clamp(Permanent, Min, Max);
}

public record CharacterAttributes(
    CharacterAttribute Musculature,
    CharacterAttribute Flexibility,
    CharacterAttribute Brain,
    CharacterAttribute Vitality)
{
    public static readonly CharacterAttributes Empty = new(
        new CharacterAttribute(0), new CharacterAttribute(0),
        new CharacterAttribute(0), new CharacterAttribute(0));

    /// <summary>
    /// Applique un modificateur avec variance aléatoire.
    /// randomBetweenZeroAnd(+2) → 0, 1 ou 2 (signe conservé).
    /// </summary>
    public CharacterAttributes ApplyModifier(AttributesModifier mod) => new(
        new CharacterAttribute(Musculature.Permanent + RandomBounded(mod.Musculature)),
        new CharacterAttribute(Flexibility.Permanent + RandomBounded(mod.Flexibility)),
        new CharacterAttribute(Brain.Permanent + RandomBounded(mod.Brain)),
        new CharacterAttribute(Vitality.Permanent + RandomBounded(mod.Vitality)));

    /// <summary>Applique un modificateur fixe (sans aléatoire).</summary>
    public CharacterAttributes ApplyFixed(AttributesModifier mod) => new(
        new CharacterAttribute(Musculature.Permanent + mod.Musculature),
        new CharacterAttribute(Flexibility.Permanent + mod.Flexibility),
        new CharacterAttribute(Brain.Permanent + mod.Brain),
        new CharacterAttribute(Vitality.Permanent + mod.Vitality));

    private static int RandomBounded(int value)
    {
        if (value == 0) return 0;
        int abs = Math.Abs(value);
        int sign = value > 0 ? 1 : -1;
        return Random.Shared.Next(0, abs + 1) * sign;
    }
}

public record AttributesModifier(
    int Musculature = 0,
    int Flexibility = 0,
    int Brain = 0,
    int Vitality = 0)
{
    public static readonly AttributesModifier Zero = new();

    public static AttributesModifier operator +(AttributesModifier a, AttributesModifier b) =>
        new(a.Musculature + b.Musculature,
            a.Flexibility + b.Flexibility,
            a.Brain + b.Brain,
            a.Vitality + b.Vitality);
}