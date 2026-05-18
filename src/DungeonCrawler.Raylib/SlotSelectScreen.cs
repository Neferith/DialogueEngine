using DungeonCrawler.MapLoader;
using Raylib_cs;

namespace DungeonCrawler.RaylibGame;

public class SlotSelectScreen : IGameScreen
{
    private readonly CampaignConfig _config;
    private readonly SaveManager _saveManager;
    private readonly bool _isNewGame;

    private SaveFile?[] _slots = [];
    private IGameScreen? _nextScreen;

    public SlotSelectScreen(CampaignConfig config, SaveManager saveManager, bool isNewGame)
    {
        _config = config;
        _saveManager = saveManager;
        _isNewGame = isNewGame;
    }

    public void OnEnter()
    {
        _slots = _saveManager.GetAllSlots();
    }

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
        string title = _isNewGame ? "Nouvelle partie" : "Charger une partie";
        var tSize = FantasyUI.MeasureText(title, 36);
        FantasyUI.Title(title, (w - tSize.X) / 2f, h * 0.08f, 36, colors);

        // ── Slots ─────────────────────────────────────────────────────────────
        float slotW = Math.Min(600f, w * 0.6f);
        float slotH = 72f;
        float slotX = (w - slotW) / 2f;
        float gap = 12f;
        float startY = h * 0.22f;

        for (int i = 0; i < _slots.Length; i++)
        {
            var save = _slots[i];
            var rect = new Rectangle(slotX, startY + i * (slotH + gap), slotW, slotH);

            bool canClick = _isNewGame || save != null;

            DrawSlot(rect, i, save, colors, canClick);

            if (canClick && SlotClicked(rect))
            {
                if (_isNewGame)
                    _nextScreen = new CharacterCreationScreen(_config, _saveManager, i);
                else if (save != null)
                    _nextScreen = LoadGame(save, i);
            }
        }

        // ── Retour ────────────────────────────────────────────────────────────
        float btnW = 200f, btnH = 48f;
        if (FantasyUI.Button(
                new Rectangle((w - btnW) / 2f, h * 0.88f, btnW, btnH),
                "← Retour", colors))
            _nextScreen = new MainMenuScreen(_config, _saveManager);
    }

    // ── Helpers ───────────────────────────────────────────────────────────────

    private void DrawSlot(Rectangle rect, int index, SaveFile? save,
                           RaylibColorScheme colors, bool active)
    {
        var borderColor = active ? colors.Primary : Darken(colors.Primary, 50);
        Raylib.DrawRectangleRounded(rect, 0.15f, 8, colors.Surface);
        Raylib.DrawRectangleRoundedLines(rect, 0.15f, 8, active ? 1.5f : 1f, borderColor);

        // Hover highlight
        if (active && Raylib.CheckCollisionPointRec(Raylib.GetMousePosition(), rect))
            Raylib.DrawRectangleRounded(rect, 0.15f, 8, new Color(255, 255, 255, 15));

        float textX = rect.X + 20;
        float midY = rect.Y + rect.Height / 2f;

        // Numéro du slot
        FantasyUI.Label($"Slot {index + 1}", textX, midY - 20, 14, colors,
                        colorOverride: colors.TextMuted);

        if (save == null)
        {
            FantasyUI.Label("— Vide —", textX + 80, midY - 10, 20, colors,
                            colorOverride: active ? colors.Text : Darken(colors.Text, 60));
        }
        else
        {
            FantasyUI.Label(save.HeroName, textX + 80, midY - 20, 22, colors);
            FantasyUI.Label($"{save.Location.MapId}  ·  {save.SavedAt:dd/MM/yyyy HH:mm}",
                            textX + 80, midY + 6, 14, colors,
                            colorOverride: colors.TextMuted);
        }
    }

    private static bool SlotClicked(Rectangle rect) =>
        Raylib.CheckCollisionPointRec(Raylib.GetMousePosition(), rect)
        && Raylib.IsMouseButtonReleased(MouseButton.Left);

    private IGameScreen LoadGame(SaveFile save, int slotIndex)
    {
        var loader = new MapFileLoader();
        var loaded = loader.Load(
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

        return new PlayingScreen(session, _config, _saveManager, slotIndex, save.HeroName);
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

    private static Color Darken(Color c, int v) =>
        new(Clamp(c.R - v), Clamp(c.G - v), Clamp(c.B - v), c.A);

    private static byte Clamp(int v) => (byte)Math.Clamp(v, 0, 255);
}