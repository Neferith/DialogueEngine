using Raylib_cs;

namespace DungeonCrawler.RaylibGame;

public class PauseOverlay
{
    private readonly CampaignConfig _config;
    private readonly ActiveSave _activeSave;
    private readonly GameServices _services;

    public bool IsActive { get; private set; }
    public IGameScreen? NextScreen { get; private set; }

    public PauseOverlay(CampaignConfig config, ActiveSave activeSave, GameServices services)
    {
        _config = config;
        _activeSave = activeSave; 
        _services = services;
    }

    public void Toggle() => IsActive = !IsActive;
    public void Close() => IsActive = false;

    public void ClearNextScreen() => NextScreen = null;

    public void Update()
    {
        if (!IsActive) return;
        if (Raylib.IsKeyPressed(KeyboardKey.Escape))
            IsActive = false;
    }

    public void Draw(int w, int h)
    {
        if (!IsActive) return;

        var colors = _config.Colors;

        // Fond semi-transparent
        Raylib.DrawRectangle(0, 0, w, h, new Color(0, 0, 0, 160));

        // Panneau central
        float panW = 320f, panH = 280f;
        float panX = (w - panW) / 2f;
        float panY = (h - panH) / 2f;
        var rect = new Rectangle(panX, panY, panW, panH);

        FantasyUI.Panel(rect, colors);

        // Titre
        var title = "— Pause —";
        var titleW = FantasyUI.MeasureText(title, 22f).X;
        FantasyUI.Title(title, panX + (panW - titleW) / 2f, panY + 20f, 22f, colors);

        // Boutons
        float btnW = panW - 48f;
        float btnX = panX + 24f;
        float btnH = 44f;
        float gap = 12f;
        float btnY = panY + 72f;

        if (FantasyUI.Button(new Rectangle(btnX, btnY, btnW, btnH),
                             "▶  Reprendre", colors))
            IsActive = false;

        if (FantasyUI.Button(new Rectangle(btnX, btnY + btnH + gap, btnW, btnH),
                             "💾  Sauvegarder", colors))
            RequestSave();

        if (FantasyUI.Button(new Rectangle(btnX, btnY + (btnH + gap) * 2, btnW, btnH),
                             "✕  Menu principal", colors))
            RequestMainMenu();
    }

    private void RequestSave()
    {
        // Déclenche la sauvegarde — PlayingScreen l'intercepte via NextScreen
        NextScreen = null; // on reste, juste save
        _saveRequested = true;
    }

    private void RequestMainMenu()
    {
        NextScreen = new MainMenuScreen(_config,
            _services); 
        IsActive = false;
    }

    private bool _saveRequested;
    public bool SaveRequested
    {
        get { var v = _saveRequested; _saveRequested = false; return v; }
    }
}