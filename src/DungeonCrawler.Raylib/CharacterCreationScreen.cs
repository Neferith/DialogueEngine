using System.Numerics;
using DungeonCrawler.Characters;
using DungeonCrawler.Characters.Backgrounds;
using DungeonCrawler.Characters.Creation;
using DungeonCrawler.Characters.Models;
using DungeonCrawler.Core.Characters;
using DungeonCrawler.Core.Entities;
using DungeonCrawler.Core.Systems;
using DungeonCrawler.MapLoader;
using Raylib_cs;

namespace DungeonCrawler.RaylibGame;

public class CharacterCreationScreen : IGameScreen
{
    private readonly CampaignConfig _config;
    private readonly SaveManager _saveManager;
    private readonly int _slotIndex;

    private CharacterBuilder _builder = null!;
    private IGameScreen? _nextScreen;

    public CharacterCreationScreen(CampaignConfig config, SaveManager saveManager, int slotIndex)
    {
        _config = config;
        _saveManager = saveManager;
        _slotIndex = slotIndex;
    }

    // ── IGameScreen ───────────────────────────────────────────────────────────

    public void OnEnter()
    {
        var loader = new BackgroundLoader();
        var rules = loader.Load(_config.CharacterRulesPath);
        _builder = new CharacterBuilder(rules.BackgroundTypes);
    }

    public void OnExit() { }

    public IGameScreen? Update(float dt)
    {
        // Saisie clavier sur l'étape Name
        if (_builder.CurrentStep == CreationStep.Name)
        {
            if (Raylib.IsKeyPressed(KeyboardKey.Tab))
                _firstnameActive = !_firstnameActive;

            if (_firstnameActive)
                _builder.SetFirstname(FantasyUI.HandleTextInput(_builder.Firstname));
            else
                _builder.SetLastname(FantasyUI.HandleTextInput(_builder.Lastname));
        }

        if (Raylib.IsKeyPressed(KeyboardKey.Enter) && _builder.IsCurrentStepValid())
            Advance();

        if (Raylib.IsKeyPressed(KeyboardKey.Escape))
            _nextScreen = new SlotSelectScreen(_config, _saveManager, isNewGame: true);

        var next = _nextScreen;
        _nextScreen = null;
        return next;
    }

    public void Draw(int w, int h)
    {
        var colors = _config.Colors;
        int panelY = 80;
        int panelH = h - panelY - 80;

        // ── Titre + breadcrumb ─────────────────────────────────────────────────
        DrawBreadcrumb(w, colors);

        // ── 3 panneaux ─────────────────────────────────────────────────────────
        int leftW = (int)(w * 0.22f);
        int rightW = (int)(w * 0.25f);
        int centerW = w - leftW - rightW;

        DrawSummaryPanel(new Rectangle(0, panelY, leftW, panelH), colors);
        DrawSelectionPanel(new Rectangle(leftW, panelY, centerW, panelH), colors);
        DrawStatsPanel(new Rectangle(leftW + centerW, panelY, rightW, panelH), colors);

        // ── Navigation ─────────────────────────────────────────────────────────
        DrawNavigation(w, h, colors);
    }

    // ── Breadcrumb ────────────────────────────────────────────────────────────

    private static readonly string[] StepLabels =
        ["Nom", "Genre", "Taille", "Poids", "Sensibilité", "Passé"];

    private void DrawBreadcrumb(int w, RaylibColorScheme colors)
    {
        float x = 20;
        float y = 20;
        float size = 16f;

        for (int i = 0; i < StepLabels.Length; i++)
        {
            bool isCurrent = (int)_builder.CurrentStep == i;
            bool isDone = (int)_builder.CurrentStep > i;

            var color = isCurrent ? colors.Accent
                      : isDone ? colors.Text
                                  : colors.TextMuted;

            FantasyUI.Label(StepLabels[i], x, y, size, colors, colorOverride: color);
            x += FantasyUI.MeasureText(StepLabels[i], size).X;

            if (i < StepLabels.Length - 1)
            {
                FantasyUI.Label(" › ", x, y, size, colors, colorOverride: colors.TextMuted);
                x += FantasyUI.MeasureText(" › ", size).X;
            }
        }
    }

    // ── Panneau gauche — Résumé ───────────────────────────────────────────────

    private void DrawSummaryPanel(Rectangle rect, RaylibColorScheme colors)
    {
        FantasyUI.Panel(rect, colors);
        float x = rect.X + 12;
        float y = rect.Y + 14;
        float lineH = 26f;

        FantasyUI.Label("Résumé", x, y, 16f, colors, colorOverride: colors.Accent);
        y += lineH + 4;

        void Row(string label, string? value)
        {
            if (value == null) return;
            FantasyUI.Label($"▸ {label}", x, y, 13f, colors, colorOverride: colors.TextMuted);
            y += 18f;
            FantasyUI.Label($"  {value}", x, y, 14f, colors);
            y += lineH;
        }

        var name = (_builder.Firstname + " " + _builder.Lastname).Trim();
        if (name.Length > 0) Row("Nom", name);

        if (_builder.Gender.HasValue)
            Row("Genre", _builder.Gender.Value == CharacterGender.Male ? "Homme" : "Femme");

        if (_builder.Size.HasValue)
            Row("Taille", _builder.Size.Value.ToString());

        if (_builder.Weight.HasValue)
            Row("Poids", _builder.Weight.Value.ToString());

        if (_builder.Sensitivity.HasValue)
            Row("Sensibilité", _builder.Sensitivity.Value.ToString());
    }

    // ── Panneau centre — Sélection ────────────────────────────────────────────

    private bool _firstnameActive = true;

    private void DrawSelectionPanel(Rectangle rect, RaylibColorScheme colors)
    {
        float px = rect.X + 16;
        float py = rect.Y + 16;

        switch (_builder.CurrentStep)
        {
            case CreationStep.Name:
                DrawNameStep(px, py, rect.Width - 32, colors);
                break;
            case CreationStep.Gender:
                DrawEnumCards(px, py, rect.Width - 32, colors,
                    new[] { "Homme", "Femme" },
                    new[] { "Musculature +1, Vitalité +1", "Flexibilité +2" },
                    i => i == (int?)_builder.Gender,
                    i => _builder.SetGender(i == 0 ? CharacterGender.Male : CharacterGender.Female));
                break;
            case CreationStep.Size:
                DrawOptionCards(px, py, rect.Width - 32, colors,
                    _builder.AvailableSizes.Select(s => s.ToString()).ToArray(),
                    i => _builder.AvailableSizes[i] == _builder.Size,
                    i => _builder.SetSize(_builder.AvailableSizes[i]));
                break;
            case CreationStep.Weight:
                DrawOptionCards(px, py, rect.Width - 32, colors,
                    _builder.AvailableWeights.Select(s => s.ToString()).ToArray(),
                    i => _builder.AvailableWeights[i] == _builder.Weight,
                    i => _builder.SetWeight(_builder.AvailableWeights[i]));
                break;
            case CreationStep.Sensitivity:
                DrawOptionCards(px, py, rect.Width - 32, colors,
                    _builder.AvailableSensitivities.Select(s => s.ToString()).ToArray(),
                    i => _builder.AvailableSensitivities[i] == _builder.Sensitivity,
                    i => _builder.SetSensitivity(_builder.AvailableSensitivities[i]));
                break;
            case CreationStep.Background:
                DrawBackgroundCards(px, py, rect.Width - 32, colors);
                break;
        }
    }

    private void DrawNameStep(float x, float y, float w, RaylibColorScheme colors)
    {
        FantasyUI.Label("Quel est le nom de ton personnage ?", x, y, 20f, colors);
        y += 40f;

        var fnRect = new Rectangle(x, y, w, 48f);
        FantasyUI.TextInput(fnRect, "Prénom", _builder.Firstname, _firstnameActive, colors, 20f);

        y += 72f;
        var lnRect = new Rectangle(x, y, w, 48f);
        FantasyUI.TextInput(lnRect, "Nom de famille", _builder.Lastname, !_firstnameActive, colors, 20f);

        // Clic pour switcher le focus
        if (Raylib.IsMouseButtonReleased(MouseButton.Left))
        {
            if (Raylib.CheckCollisionPointRec(Raylib.GetMousePosition(), fnRect))
                _firstnameActive = true;
            else if (Raylib.CheckCollisionPointRec(Raylib.GetMousePosition(), lnRect))
                _firstnameActive = false;
        }
    }

    private void DrawEnumCards(float x, float y, float w, RaylibColorScheme colors,
        string[] labels, string[] subtitles,
        Func<int, bool> isSelected, Action<int> onSelect)
    {
        float cardW = (w - 12f) / 2f;
        float cardH = 80f;

        for (int i = 0; i < labels.Length; i++)
        {
            float cx = x + i * (cardW + 12f);
            if (FantasyUI.SelectableCard(
                    new Rectangle(cx, y, cardW, cardH),
                    labels[i], subtitles[i], isSelected(i), colors))
                onSelect(i);
        }
    }

    private void DrawOptionCards(float x, float y, float w, RaylibColorScheme colors,
        string[] labels, Func<int, bool> isSelected, Action<int> onSelect)
    {
        float cardW = (w - 12f * 2f) / 3f;
        float cardH = 60f;
        float gap = 12f;
        int cols = 3;

        for (int i = 0; i < labels.Length; i++)
        {
            float cx = x + (i % cols) * (cardW + gap);
            float cy = y + (i / cols) * (cardH + gap);
            if (FantasyUI.SelectableCard(
                    new Rectangle(cx, cy, cardW, cardH),
                    labels[i], null, isSelected(i), colors))
                onSelect(i);
        }
    }

    private void DrawBackgroundCards(float x, float y, float w, RaylibColorScheme colors)
    {
        var bgs = _builder.AvailableBackgrounds;
        var type = _builder.CurrentBackgroundType;
        float cardW = w;
        float cardH = 72f;
        float gap = 10f;

        if (type != null)
        {
            FantasyUI.Label(type.Name, x, y - 4f, 14f, colors,
                colorOverride: colors.Accent);
            y += 20f;
        }

        for (int i = 0; i < bgs.Count; i++)
        {
            var bg = bgs[i];
            bool selected = _builder.CurrentBackground?.Id == bg.Id;

            if (FantasyUI.SelectableCard(
                    new Rectangle(x, y + i * (cardH + gap), cardW, cardH),
                    bg.Name, bg.Description, selected, colors))
                _builder.SetBackground(bg);
        }
    }

    // ── Panneau droit — Stats ─────────────────────────────────────────────────

    private void DrawStatsPanel(Rectangle rect, RaylibColorScheme colors)
    {
        FantasyUI.Panel(rect, colors);

        float x = rect.X + 12;
        float y = rect.Y + 14;
        float lineH = 22f;
        var attrs = _builder.CurrentAttributes;

        FantasyUI.Label("Attributs", x, y, 16f, colors, colorOverride: colors.Accent);
        y += lineH + 4;

        void StatRow(string label, int value)
        {
            var color = value > 0 ? new Color(100, 200, 120, 255)
                      : value < 0 ? new Color(200, 90, 90, 255)
                                  : colors.TextMuted;
            FantasyUI.Label(label, x, y, 14f, colors);
            var valStr = value > 0 ? $"+{value}" : value.ToString();
            var valW = FantasyUI.MeasureText(valStr, 14f).X;
            FantasyUI.Label(valStr, rect.X + rect.Width - valW - 12, y, 14f, colors,
                colorOverride: color);
            y += lineH;
        }

        StatRow("Musculature", attrs.Musculature.Current());
        StatRow("Flexibilité", attrs.Flexibility.Current());
        StatRow("Intelligence", attrs.Brain.Current());
        StatRow("Vitalité", attrs.Vitality.Current());

        y += 10f;
        Raylib.DrawLine((int)rect.X + 8, (int)y, (int)(rect.X + rect.Width) - 8,
            (int)y, colors.Primary);
        y += 12f;

        FantasyUI.Label("Combat", x, y, 16f, colors, colorOverride: colors.Accent);
        y += lineH + 4;

        // Stats dérivées (preview)
        int mus = attrs.Musculature.Current();
        int fle = attrs.Flexibility.Current();
        int bra = attrs.Brain.Current();
        int vit = attrs.Vitality.Current();

        StatRow("PV max", vit * 2 + mus + Character.BasePv);
        StatRow("QAm", mus * 2 + vit);
        StatRow("QAc", bra * 2 + fle);
        StatRow("QDp", vit * 2 + mus);
        StatRow("QDe", fle * 2 + bra);
    }

    // ── Navigation ────────────────────────────────────────────────────────────

    private void DrawNavigation(int w, int h, RaylibColorScheme colors)
    {
        float btnW = 180f, btnH = 48f;
        float btnY = h - 64f;

        if (FantasyUI.Button(new Rectangle(w / 2f - btnW - 10f, btnY, btnW, btnH),
                             "← Retour", colors))
        {
            if (_builder.CurrentStep == CreationStep.Name)
                _nextScreen = new SlotSelectScreen(_config, _saveManager, isNewGame: true);
        }

        string nextLabel = _builder.IsLastStep ? "✦  Commencer" : "Suivant →";
        bool canNext = _builder.IsCurrentStepValid();

        if (FantasyUI.Button(new Rectangle(w / 2f + 10f, btnY, btnW, btnH),
                             nextLabel, colors) && canNext)
            Advance();
    }

    // ── Avancer ───────────────────────────────────────────────────────────────

    private void Advance()
    {
        if (_builder.IsLastStep && _builder.IsCurrentStepValid())
        {
            _builder.NextStep();
            if (_builder.IsComplete)
                StartGame();
            return;
        }

        _builder.NextStep();
    }

    private void StartGame()
    {
        var character = _builder.Build();

        var save = new SaveFile
        {
            SlotName = $"Slot {_slotIndex + 1}",
            HeroName = character.Description.Name.FullName,
            Location = new LocationSave
            {
                MapId = _config.StartingMapId,
                X = 0,
                Y = 0,
                Facing = "NORTH"
            }
        };

        _saveManager.Save(_slotIndex, save);
        _nextScreen = BuildPlayingScreen(save);
    }

    private IGameScreen BuildPlayingScreen(SaveFile save,
        DungeonCrawler.MapLoader.LoadedMap? preloaded = null,
        MapFileLoader? existingLoader = null)
    {
        var loader = existingLoader ?? new MapFileLoader();
        var loaded = preloaded ?? loader.Load(
            Path.Combine(_config.MapsPath, $"{save.Location.MapId}.map.json"),
            _config.ModulesPath);

        var spawn = loaded.PlayerSpawn ?? new DungeonCrawler.Core.Models.GridPosition(1, 1);
        var facing = loaded.PlayerFacing;

        var party = new DungeonCrawler.Core.Characters.Party(spawn, facing, maxSize: 4);
        party.TryAddMember(new DungeonCrawler.Core.Characters.PartyMember(save.HeroName));

        var entities = new EntitySystem();
        var runner = new DungeonCrawler.Core.DungeonRunner(loaded.Map, party, entities);
        var turns = new TurnManager(runner, entities);
        var session = new DungeonSession(loaded, runner, turns, loader,
                           _config.MapsPath, _config.ModulesPath);

        return new PlayingScreen(session, _config,
            new ActiveSave(_saveManager, _slotIndex, save.HeroName));
    }
}