using DungeonCrawler.Characters.Creation;
using DungeonCrawler.Characters.Models;
using DungeonCrawler.Characters.Skills;
using DungeonCrawler.Core;
using DungeonCrawler.Persistence;

namespace DungeonCrawler.RaylibGame;

public static class CharacterMapper
{
    // ── Character → CharacterSaveData ─────────────────────────────────────────

    public static CharacterSaveData ToSaveData(Character c) => new()
    {
        Id = c.Id,
        FactionId = c.FactionId,
        Firstname = c.Description.Name.Firstname,
        Lastname = c.Description.Name.Lastname,
        Age = c.Description.Age,
        Gender = c.Description.Gender.ToString(),
        Size = c.Description.Size.ToString(),
        Weight = c.Description.Weight.ToString(),
        Sensitivity = c.Description.Sensitivity.ToString(),
        BackgroundId = c.Description.Background?.Id ?? "",

        Musculature = c.Attributes.Musculature.Permanent,
        Flexibility = c.Attributes.Flexibility.Permanent,
        Brain = c.Attributes.Brain.Permanent,
        Vitality = c.Attributes.Vitality.Permanent,

        CurrentHp = c.State.CurrentHp,
        Injuries = c.State.Injuries.Select(ToInjurySaveData).ToList(),

        Level = c.Level.Level,
        Experience = c.Level.Experience,

        SkillIds = c.Skills.All.Select(s => s.SkillId).ToList()
    };

    // ── CharacterSaveData → Character ─────────────────────────────────────────

    public static Character FromSaveData(CharacterSaveData d)
    {
        var attrs = new CharacterAttributes(
            new CharacterAttribute(d.Musculature),
            new CharacterAttribute(d.Flexibility),
            new CharacterAttribute(d.Brain),
            new CharacterAttribute(d.Vitality));

        var desc = new CharacterDescription(
            Name: new CharacterName(d.Firstname, d.Lastname),
            Age: d.Age,
            Gender: Enum.TryParse<CharacterGender>(d.Gender, out var g)
                         ? g : CharacterGender.Male,
            Size: Enum.TryParse<CharacterSize>(d.Size, out var s)
                         ? s : CharacterSize.Medium,
            Weight: Enum.TryParse<CharacterWeight>(d.Weight, out var w)
                         ? w : CharacterWeight.Average,
            Sensitivity: Enum.TryParse<CharacterSensitivity>(d.Sensitivity, out var sens)
                         ? sens : CharacterSensitivity.Normal,
            Background: null); // background rechargé depuis les rules si besoin

        var character = Character.Create(desc, attrs, d.FactionId);

        // Restaurer l'état HP
        var state = new CharacterState(d.CurrentHp,
            d.Injuries.Select(FromInjurySaveData)
                      .Where(i => i != null)
                      .Cast<Injury>()
                      .ToList());

        return character.WithState(state);
    }

    // ── Injury ────────────────────────────────────────────────────────────────

    private static InjurySaveData ToInjurySaveData(Injury injury) => injury switch
    {
        Injury.Physical p => new InjurySaveData
        {
            Severity = p.Severity.ToString(),
            Family = "Physical",
            SubType = p.GetType().Name,
            Detail = p.Location.ToString()
        },
        Injury.Mental m => new InjurySaveData
        {
            Severity = m.Severity.ToString(),
            Family = "Mental",
            SubType = m.GetType().Name,
            Detail = m.Effect.ToString()
        },
        Injury.Energy e => new InjurySaveData
        {
            Severity = e.Severity.ToString(),
            Family = "Energy",
            SubType = e.GetType().Name,
            Detail = e.Source.ToString()
        },
        _ => new InjurySaveData { Severity = injury.Severity.ToString() }
    };

    private static Injury? FromInjurySaveData(InjurySaveData d)
    {
        if (!Enum.TryParse<InjurySeverity>(d.Severity, out var sev)) return null;

        return d.Family switch
        {
            "Physical" => Enum.TryParse<Injury.BodyLocation>(d.Detail, out var loc)
                ? d.SubType switch
                {
                    "Cut" => (Injury)new Injury.Physical.Cut(sev, loc),
                    "Fracture" => new Injury.Physical.Fracture(sev, loc),
                    "Burn" => new Injury.Physical.Burn(sev, loc),
                    _ => new Injury.Physical.Cut(sev, loc)
                } : null,

            "Mental" => Enum.TryParse<Injury.MentalEffect>(d.Detail, out var eff)
                ? d.SubType switch
                {
                    "Shock" => (Injury)new Injury.Mental.Shock(sev, eff),
                    "Madness" => new Injury.Mental.Madness(sev, eff),
                    _ => new Injury.Mental.Shock(sev, eff)
                } : null,

            "Energy" => Enum.TryParse<Injury.EnergySource>(d.Detail, out var src)
                ? d.SubType switch
                {
                    "EnergyBurn" => (Injury)new Injury.Energy.EnergyBurn(sev, src),
                    "Corruption" => new Injury.Energy.Corruption(sev, src),
                    _ => new Injury.Energy.EnergyBurn(sev, src)
                } : null,

            _ => null
        };
    }
}