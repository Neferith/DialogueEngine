using System.Text.Json.Serialization;

namespace MapEditor.Core.Models;

public class CampaignProject
{
    public string Name { get; set; } = "";
    public string ModulesPath { get; set; } = "modules";
    public string MapsPath { get; set; } = "maps";

    public string CharacterRulesPath { get; set; } = "rules/character_rules.json";

    public string EventsPath { get; set; } = "events";

    public string ItemsPath { get; init; } = "items/items.json";

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

    [JsonIgnore]
    public string AbsoluteEventsPath =>
    ProjectDirectory != null
        ? Path.Combine(ProjectDirectory, EventsPath)
        : EventsPath;


    [JsonIgnore]
    public string AbsoluteItemsPath =>
ProjectDirectory != null
    ? Path.Combine(ProjectDirectory, ItemsPath)
    : ItemsPath;
}