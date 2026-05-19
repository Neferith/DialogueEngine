using System.Text.Json.Serialization;

namespace MapEditor.Core.Models;

public class CampaignProject
{
    public string Name { get; set; } = "";
    public string ModulesPath { get; set; } = "modules";
    public string MapsPath { get; set; } = "maps";

    public string CharacterRulesPath { get; set; } = "character_rules.json";

    // ── Runtime (non sérialisé) ───────────────────────────────────────────────

    [JsonIgnore]
    public string? ProjectFilePath { get; set; }

    [JsonIgnore]
    public string? ProjectDirectory =>
        ProjectFilePath != null ? Path.GetDirectoryName(ProjectFilePath) : null;

    [JsonIgnore]
    public string AbsoluteModulesPath =>
        ProjectDirectory != null
            ? Path.Combine(ProjectDirectory, ModulesPath)
            : ModulesPath;

    [JsonIgnore]
    public string AbsoluteMapsPath =>
        ProjectDirectory != null
            ? Path.Combine(ProjectDirectory, MapsPath)
            : MapsPath;
    
    [JsonIgnore]
    public string AbsoluteCharacterRulesPath =>
    ProjectDirectory != null
            ? Path.Combine(ProjectDirectory, CharacterRulesPath): CharacterRulesPath;
}