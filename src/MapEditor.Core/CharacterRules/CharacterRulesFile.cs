namespace MapEditor.Core.Characters;

public class CharacterRulesFile
{
    public List<BackgroundTypeData> BackgroundTypes { get; set; } = new();
    public List<SkillData> Skills { get; set; } = new();
}

public class BackgroundTypeData
{
    public string Id { get; set; } = "";
    public string Name { get; set; } = "";
    public string Description { get; set; } = "";
    public List<BackgroundData> Backgrounds { get; set; } = new();
}

public class BackgroundData
{
    public string Id { get; set; } = "";
    public string Name { get; set; } = "";
    public string Description { get; set; } = "";
    public string TypeId { get; set; } = "";
    public AttributesModifierData AttributesModifier { get; set; } = new();
    public List<string> StartingSkillIds { get; set; } = new();
    public BackgroundRequirementData? Requirement { get; set; }
}

public class BackgroundRequirementData
{
    public List<string>? AllowedSizes { get; set; }
}

public class AttributesModifierData
{
    public int Musculature { get; set; }
    public int Flexibility { get; set; }
    public int Brain { get; set; }
    public int Vitality { get; set; }
}

public class SkillData
{
    public string Id { get; set; } = "";
    public string Name { get; set; } = "";
    public string Type { get; set; } = "";
    public string Description { get; set; } = "";
    public AttributesModifierData Modifier { get; set; } = new();
}