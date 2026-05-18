using DungeonCrawler.MapLoader;
using Raylib_cs;

namespace DungeonCrawler.RaylibGame;

public class CharacterCreationScreen : IGameScreen
{
    private readonly CampaignConfig _config;
    private readonly SaveManager _saveManager;
    private readonly int _slotIndex;

    private string _heroName = "";
    private bool _inputFocus = true;
    private IGameScreen? _nextScreen;

    public CharacterCreationScreen(CampaignConfig config, SaveManager saveManager, int slotIndex)
    {
        _config = config;
        _saveManager = saveManager;
        _slotIndex = slotIndex;
    }

    public void OnEnter() { }
    public void OnExit() { }

    public IGameScreen? Update(float dt)
    {
        // Saisie clavier
        if (_inputFocus)
            _heroName = FantasyUI.HandleTextInput(_heroName, maxLength: 20);

        // Entrée pour valider
        if (Raylib.IsKeyPressed(KeyboardKey.Enter) && _heroName.Trim().Length > 0)
            StartGame();

        var next = _nextScreen;
        _nextScreen = null;
        return next;
    }

    public void Draw(int w, int h)
    {
        var colors = _config.Colors;

        // ── Titre ─────────────────────────────────────────────────────────────
        string title = "Création du personnage";
        var tSize = FantasyUI.MeasureText(title, 32);
        FantasyUI.Title(title, (w - tSize.X) / 2f, h * 0.10f, 32, colors);

        // ── Panneau central ───────────────────────────────────────────────────
        float panelW = Math.Min(480f, w * 0.5f);
        float panelH = 220f;
        float panelX = (w - panelW) / 2f;
        float panelY = h * 0.30f;

        FantasyUI.Panel(new Rectangle(panelX, panelY, panelW, panelH), colors);

        // ── Champ nom ─────────────────────────────────────────────────────────
        float fieldW = panelW - 60f;
        float fieldH = 52f;
        var fieldRect = new Rectangle(
            panelX + 30f,
            panelY + 80f,
            fieldW, fieldH);

        FantasyUI.Label("Quel est ton nom, aventurier ?",
            panelX + 30f, panelY + 30f, 18, colors);

        FantasyUI.TextInput(fieldRect, "Nom du héros", _heroName, _inputFocus, colors, 22f);

        // Clic sur le champ = focus
        if (Raylib.IsMouseButtonReleased(MouseButton.Left))
            _inputFocus = Raylib.CheckCollisionPointRec(
                Raylib.GetMousePosition(), fieldRect);

        // Hint
        if (_heroName.Length == 0)
            FantasyUI.Label("Appuie sur Entrée pour commencer",
                panelX + 30f, panelY + 160f, 13, colors,
                colorOverride: colors.TextMuted);

        // ── Boutons ───────────────────────────────────────────────────────────
        float btnW = 200f, btnH = 50f;
        float btnY = h * 0.72f;

        bool canConfirm = _heroName.Trim().Length > 0;

        // Bouton Commencer
        var confirmColors = canConfirm ? colors : colors with
        {
            Primary = new Raylib_cs.Color(50, 35, 25, 255),
            Accent = new Raylib_cs.Color(100, 80, 55, 255)
        };

        if (FantasyUI.Button(
                new Rectangle((w / 2f) + 10f, btnY, btnW, btnH),
                "✦  Commencer", confirmColors) && canConfirm)
            StartGame();

        // Bouton Retour
        if (FantasyUI.Button(
                new Rectangle((w / 2f) - btnW - 10f, btnY, btnW, btnH),
                "← Retour", colors))
            _nextScreen = new SlotSelectScreen(_config, _saveManager, isNewGame: true);
    }

    // ── Démarrage ─────────────────────────────────────────────────────────────

    private void StartGame()
    {
        var loader = new MapFileLoader();
        var loaded = loader.Load(
            Path.Combine(_config.MapsPath, $"{_config.StartingMapId}.map.json"),
            _config.ModulesPath);

        var spawn = loaded.PlayerSpawn ?? new DungeonCrawler.Core.Models.GridPosition(1, 1);
        var facing = loaded.PlayerFacing;

        var save = new SaveFile
        {
            SlotName = $"Slot {_slotIndex + 1}",
            HeroName = _heroName.Trim(),
            Location = new LocationSave
            {
                MapId = _config.StartingMapId,
                X = spawn.X,
                Y = spawn.Y,
                Facing = facing.ToString().ToUpperInvariant()
            }
        };

        _saveManager.Save(_slotIndex, save);
        _nextScreen = BuildPlayingScreen(save, loaded, loader); // ← passer loader
    }

    private IGameScreen BuildPlayingScreen(SaveFile save,
        DungeonCrawler.MapLoader.LoadedMap? preloaded = null,
        MapFileLoader? existingLoader = null)
    {
        var loader = existingLoader ?? new MapFileLoader();
        var loaded = preloaded ?? loader.Load(
            Path.Combine(_config.MapsPath, $"{save.Location.MapId}.map.json"),
            _config.ModulesPath);

        var pos = new DungeonCrawler.Core.Models.GridPosition(save.Location.X, save.Location.Y);
        var facing = ParseDirection(save.Location.Facing);

        var party = new DungeonCrawler.Core.Characters.Party(pos, facing, maxSize: 4);
        party.TryAddMember(new DungeonCrawler.Core.Characters.PartyMember(save.HeroName));

        var entities = new DungeonCrawler.Core.Entities.EntitySystem();
        var runner = new DungeonCrawler.Core.DungeonRunner(loaded.Map, party, entities);
        var turns = new DungeonCrawler.Core.Systems.TurnManager(runner, entities);
        var session = new DungeonCrawler.MapLoader.DungeonSession(
            loaded, runner, turns, loader,
            _config.MapsPath, _config.ModulesPath);

        var activeSave = new ActiveSave(_saveManager, _slotIndex, save.HeroName);
        return new PlayingScreen(session, _config, activeSave);
    }

    private static DungeonCrawler.Core.Models.Direction ParseDirection(string facing) =>
        facing.ToUpperInvariant() switch
        {
            "NORTH" => DungeonCrawler.Core.Models.Direction.North,
            "EAST" => DungeonCrawler.Core.Models.Direction.East,
            "SOUTH" => DungeonCrawler.Core.Models.Direction.South,
            "WEST" => DungeonCrawler.Core.Models.Direction.West,
            _ => DungeonCrawler.Core.Models.Direction.North
        };
}