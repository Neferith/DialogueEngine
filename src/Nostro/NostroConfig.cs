using DungeonCrawler.RaylibGame;
using Raylib_cs;

namespace Nostro;

public class NostroConfig : CampaignConfig
{
    public NostroConfig() : base() { }

    public static CampaignConfig Create() => new CampaignConfig
    {
        Title = "Nostro",
        SaveFolderName = "Nostro",
        FontPath = "Assets/fonts/medievalsharp_regular.ttf",
        FontSize = 32,
        ModulesPath = "modules",
        MapsPath = "maps",
        AssetsPath = "Assets",
        CharacterRulesPath = "characters/character_rules.json",
        StartingMapId = "the_cells",
        Colors = new RaylibColorScheme
        {
            Background = new(27, 27, 27, 255),
            Surface = new(42, 42, 42, 255),
            SurfaceSecondary = new(43, 43, 40, 255),
            Primary = new(91, 58, 41, 255),
            Accent = new(196, 154, 108, 255),
            Text = new(224, 214, 200, 255),
            TextMuted = new(234, 224, 213, 255),
            Danger = new(140, 47, 57, 255)
        }
    };
}