using Raylib_cs;

namespace DungeonCrawler.RaylibGame;

public record RaylibColorScheme
{
    public Color Background { get; init; } = new(27, 27, 27, 255);
    public Color Surface { get; init; } = new(42, 42, 42, 255);
    public Color SurfaceSecondary { get; init; } = new(43, 43, 40, 255);
    public Color Primary { get; init; } = new(91, 58, 41, 255);
    public Color Accent { get; init; } = new(196, 154, 108, 255);
    public Color Text { get; init; } = new(224, 214, 200, 255);
    public Color TextMuted { get; init; } = new(234, 224, 213, 255);
    public Color Danger { get; init; } = new(140, 47, 57, 255);
}

public class CampaignConfig
{
    /// <summary>Titre affiché dans la fenêtre et le menu principal.</summary>
    public string Title { get; init; } = "DungeonCrawler";

    /// <summary>Nom du dossier dans %AppData% pour les sauvegardes.</summary>
    public string SaveFolderName { get; init; } = "DungeonCrawler";

    /// <summary>Chemin vers la police TTF (relatif au dossier de l'exe).</summary>
    public string FontPath { get; init; } = "";

    /// <summary>Taille de chargement de la police (pixels).</summary>
    public int FontSize { get; init; } = 32;

    /// <summary>Dossier des modules/biomes.</summary>
    public string ModulesPath { get; init; } = "modules";

    /// <summary>Dossier des maps.</summary>
    public string MapsPath { get; init; } = "maps";

    /// <summary>Dossier des assets visuels (textures).</summary>
    public string AssetsPath { get; init; } = "Assets";

    /// <summary>Chemin vers character_rules.json (relatif au dossier de l'exe).</summary>
    public string CharacterRulesPath { get; init; } = "rules/character_rules.json";

    public string DialoguesPath { get; init; } = "dialogues";

    public string EventsPath { get; init; } = "events";

    public string ItemsPath { get; init; } = "items/items.json";

    /// <summary>Map chargée au démarrage d'une nouvelle partie.</summary>
    public string StartingMapId { get; init; } = "";

    /// <summary>Palette de couleurs de la campagne.</summary>
    public RaylibColorScheme Colors { get; init; } = new();
}