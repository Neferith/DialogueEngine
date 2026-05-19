using DungeonCrawler.Characters.Creation;
using DungeonCrawler.Characters.Models;
using DungeonCrawler.Characters.Skills;
using System.Text.Json;

namespace DungeonCrawler.Characters.Backgrounds;

public class BackgroundLoader
{
    private static readonly JsonSerializerOptions _options = new()
    {
        PropertyNameCaseInsensitive = true
    };

    public CharacterRules Load(string path)
    {
        if (!File.Exists(path)) return CharacterRules.Empty;
        try
        {
            var json = File.ReadAllText(path);
            var raw = JsonSerializer.Deserialize<CharacterRulesRaw>(json, _options)
                       ?? new CharacterRulesRaw();
            return Map(raw);
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"[BackgroundLoader] Erreur : {ex.Message}");
            return CharacterRules.Empty;
        }
    }

    // ── Mapping raw → domain ──────────────────────────────────────────────────

    private static CharacterRules Map(CharacterRulesRaw raw) => new(
        BackgroundTypes: raw.BackgroundTypes.Select(MapType).ToList(),
        Skills: raw.Skills.Select(MapSkill).ToList());

    private static BackgroundType MapType(BackgroundTypeRaw t) => new()
    {
        Id = t.Id,
        Name = t.Name,
        Description = t.Description,
        Backgrounds = t.Backgrounds.Select(MapBackground).ToList()
    };

    private static Background MapBackground(BackgroundRaw b) => new()
    {
        Id = b.Id,
        Name = b.Name,
        Description = b.Description,
        TypeId = b.TypeId,
        AttributesModifier = new AttributesModifier(
            b.AttributesModifier.Musculature,
            b.AttributesModifier.Flexibility,
            b.AttributesModifier.Brain,
            b.AttributesModifier.Vitality),
        StartingSkillIds = b.StartingSkillIds,
        Requirement = b.Requirement == null ? null : new BackgroundRequirement
        {
            AllowedSizes = b.Requirement.AllowedSizes?
                .Select(s => Enum.Parse<CharacterSize>(s, ignoreCase: true))
                .ToList()
        }
    };

    private static Skill MapSkill(SkillRaw s) => new()
    {
        Id = s.Id,
        Name = s.Name,
        Description = s.Description,
        Type = Enum.TryParse<SkillType>(s.Type, ignoreCase: true, out var t)
                      ? t : SkillType.Technical,
        Modifier = new AttributesModifier(
            s.Modifier.Musculature,
            s.Modifier.Flexibility,
            s.Modifier.Brain,
            s.Modifier.Vitality)
    };

    // ── Raw models (désérialisation JSON) ─────────────────────────────────────

    private record CharacterRulesRaw(
        List<BackgroundTypeRaw> BackgroundTypes,
        List<SkillRaw> Skills)
    {
        public CharacterRulesRaw() : this(new(), new()) { }
    }

    private record BackgroundTypeRaw(
        string Id, string Name, string Description,
        List<BackgroundRaw> Backgrounds)
    {
        public BackgroundTypeRaw() : this("", "", "", new()) { }
    }

    private record BackgroundRaw(
        string Id, string Name, string Description, string TypeId,
        AttributesModifierRaw AttributesModifier,
        List<string> StartingSkillIds,
        BackgroundRequirementRaw? Requirement)
    {
        public BackgroundRaw() : this("", "", "", "",
            new AttributesModifierRaw(), new(), null)
        { }
    }

    private record BackgroundRequirementRaw(List<string>? AllowedSizes)
    {
        public BackgroundRequirementRaw() : this((List<string>?)null) { }
    }

    private record AttributesModifierRaw(
        int Musculature, int Flexibility, int Brain, int Vitality)
    {
        public AttributesModifierRaw() : this(0, 0, 0, 0) { }
    }

    private record SkillRaw(
        string Id, string Name, string Type, string Description,
        AttributesModifierRaw Modifier)
    {
        public SkillRaw() : this("", "", "", "", new()) { }
    }
}

public record CharacterRules(
    List<BackgroundType> BackgroundTypes,
    List<Skill> Skills)
{
    public static readonly CharacterRules Empty = new(new(), new());
}