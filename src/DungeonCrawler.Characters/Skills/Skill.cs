using DungeonCrawler.Characters.Models;

namespace DungeonCrawler.Characters.Skills;

public enum SkillType
{
    Weapon, Distance, Defense, Social, Technical, Knowledge, Magic
}

public class Skill
{
    public string Id { get; set; } = "";
    public string Name { get; set; } = "";
    public SkillType Type { get; set; }
    public string Description { get; set; } = "";
    public AttributesModifier Modifier { get; set; } = AttributesModifier.Zero;
}

public record CharacterSkill(string SkillId, int Level);

public class CharacterSkills
{
    private readonly Dictionary<string, CharacterSkill> _skills;

    public CharacterSkills() => _skills = new();

    public CharacterSkills(IEnumerable<CharacterSkill> skills)
        => _skills = skills.ToDictionary(s => s.SkillId);

    public CharacterSkills Add(CharacterSkill skill) =>
        new(_skills.Values.Append(skill));

    public CharacterSkill? Get(string id) =>
        _skills.TryGetValue(id, out var s) ? s : null;

    public IReadOnlyCollection<CharacterSkill> All => _skills.Values;
}