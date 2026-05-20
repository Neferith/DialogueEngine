using DungeonCrawler.Characters.Backgrounds;
using DungeonCrawler.Characters.Creation;
using DungeonCrawler.Characters.Skills;

namespace DungeonCrawler.Characters.Models;

public record CharacterName(string Firstname, string Lastname)
{
    public string FullName => $"{Firstname} {Lastname}";
    public override string ToString() => FullName;
}

public record CharacterDescription(
    CharacterName Name,
    int Age,
    CharacterGender Gender,
    CharacterSize Size,
    CharacterWeight Weight,
    CharacterSensitivity Sensitivity,
    Background? Background);

public record CharacterLevel(int Level = 1, int Experience = 0);

public class Character
{
    public string Id { get; init; } = Guid.NewGuid().ToString();
    public string FactionId { get; init; } = "";
    public CharacterAttributes Attributes { get; init; } = CharacterAttributes.Empty;
    public CharacterDescription Description { get; init; } = null!;
    public CharacterLevel Level { get; init; } = new();
    public CharacterSkills Skills { get; init; } = new();
    public CharacterState State { get; private set; } = null!;

    public const int BasePv = 15;

    // ── Initialisation ────────────────────────────────────────────────────────

    public static Character Create(
        CharacterDescription description,
        CharacterAttributes attributes,
        string factionId = "")
    {
        var c = new Character
        {
            FactionId = factionId,
            Attributes = attributes,
            Description = description,
        };
        c.State = new CharacterState(c.MaxHp());
        return c;
    }

    // ── Stats dérivées ────────────────────────────────────────────────────────

    /// <summary>PV max = (Vitality × 2 + Musculature) + BASE_PV</summary>
    public int MaxHp() =>
        Attributes.Vitality.Current() * 2
        + Attributes.Musculature.Current()
        + BasePv;

    /// <summary>QAm — Attaque Puissante = Musculature × 2 + Vitality</summary>
    public int MightyAttackQuotient() =>
        Attributes.Musculature.Current() * 2
        + Attributes.Vitality.Current();

    /// <summary>QAc — Attaque Critique = Brain × 2 + Flexibility</summary>
    public int CriticalAttackQuotient() =>
        Attributes.Brain.Current() * 2
        + Attributes.Flexibility.Current();

    /// <summary>QDp — Défense Parade = Vitality × 2 + Musculature</summary>
    public int ParryDefenseQuotient() =>
        Attributes.Vitality.Current() * 2
        + Attributes.Musculature.Current();

    /// <summary>QDe — Défense Esquive = Flexibility × 2 + Brain</summary>
    public int DodgeDefenseQuotient() =>
        Attributes.Flexibility.Current() * 2
        + Attributes.Brain.Current();

    // ── Bonus équipement (stubs) ──────────────────────────────────────────────

    public virtual int WeaponPowerBonus() => 0;
    public virtual int WeaponFinesseBonus() => 0;
    public virtual int ArmourParryBonus() => 0;
    public virtual int ArmourDodgeBonus() => 0;

    // ── Quotients finaux (attributs + équipement) ─────────────────────────────

    public int FinalMightyAttack() => MightyAttackQuotient() + WeaponPowerBonus();
    public int FinalCriticalAttack() => CriticalAttackQuotient() + WeaponFinesseBonus();
    public int FinalParryDefense() => ParryDefenseQuotient() + ArmourParryBonus();
    public int FinalDodgeDefense() => DodgeDefenseQuotient() + ArmourDodgeBonus();

    // ── Résolution des blessures (stub) ──────────────────────────────────────

    public virtual IReadOnlyList<Injury> ResolveInjuries(int rollMargin)
    {
        if (rollMargin <= 0) return [];
        return [new Injury.Physical.Cut(
            rollMargin >= 10 ? InjurySeverity.Severe
                             : rollMargin >= 5 ? InjurySeverity.Moderate
                                               : InjurySeverity.Minor,
            Injury.BodyLocation.Torso)];
    }

    // ── Mutations immutables ──────────────────────────────────────────────────

    public Character WithState(CharacterState state)
    {
        var c = (Character)MemberwiseClone();
        c.State = state;
        return c;
    }

    public Character WithDamage(int amount) => WithState(State.WithDamage(amount));
    public Character WithHeal(int amount) => WithState(State.WithHeal(amount, MaxHp()));
    public Character WithInjury(Injury injury) => WithState(State.WithInjury(injury));

    public override string ToString() => Description.Name.FullName;
}