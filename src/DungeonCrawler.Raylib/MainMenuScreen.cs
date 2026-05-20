using System.Numerics;
using DungeonCrawler.Core;
using DungeonCrawler.Persistence;
using Raylib_cs;

namespace DungeonCrawler.RaylibGame;

public class MainMenuScreen : IGameScreen
{
    private readonly CampaignConfig _config;
    private readonly GameServices _services;
    private IGameScreen? _nextScreen;

    public MainMenuScreen(CampaignConfig config, GameServices services)
    {
        _config = config;
        _services = services;
    }

    public void OnEnter() => FantasyUI.Init(_config);
    public void OnExit() { }

    public IGameScreen? Update(float dt)
    {
        var next = _nextScreen;
        _nextScreen = null;
        return next;
    }

    public void Draw(int w, int h)
    {
        var colors = _config.Colors;

        // ── Titre ─────────────────────────────────────────────────────────────
        float titleSize = h * 0.12f;
        var titleMeasure = FantasyUI.MeasureText(_config.Title, titleSize);
        FantasyUI.Title(_config.Title,
            (w - titleMeasure.X) / 2f, h * 0.12f,
            titleSize, colors);

        string sub = "— An epic dark fantasy adventure —";
        var subMeasure = FantasyUI.MeasureText(sub, 18);
        FantasyUI.Label(sub,
            (w - subMeasure.X) / 2f,
            h * 0.12f + titleMeasure.Y + 8f,
            18, colors, colorOverride: colors.TextMuted);

        // ── Boutons ───────────────────────────────────────────────────────────
        float btnW = 320f;
        float btnH = 54f;
        float btnX = (w - btnW) / 2f;
        float gap = 18f;
        float startY = h * 0.46f;

        if (FantasyUI.Button(new Rectangle(btnX, startY, btnW, btnH), "✦  Nouvelle partie", colors))
            _nextScreen = new SlotSelectScreen(_config, _services, isNewGame: true);

        bool hasSave = _services.SaveManager.GetAllSlots().Any(s => s != null);
        if (FantasyUI.Button(new Rectangle(btnX, startY + btnH + gap, btnW, btnH), "⚔  Charger une partie", colors) && hasSave)
            _nextScreen = new SlotSelectScreen(_config, _services, isNewGame: false);

        if (FantasyUI.Button(new Rectangle(btnX, startY + (btnH + gap) * 2, btnW, btnH),
                     "✕  Quitter", colors))
            Environment.Exit(0);

        // ── Version ───────────────────────────────────────────────────────────
        FantasyUI.Label("v0.1", w - 60, h - 28, 14, colors,
                        colorOverride: colors.TextMuted);
    }
}