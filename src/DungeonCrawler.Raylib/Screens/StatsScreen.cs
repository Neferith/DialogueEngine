using System.Numerics;
using DungeonCrawler.Characters.Models;
using DungeonCrawler.MapLoader;
using Raylib_cs;

namespace DungeonCrawler.RaylibGame;

public class StatsScreen : IGameScreen
{
    private readonly DungeonSession _session;
    private readonly CampaignConfig _config;
    private readonly ActiveSave _activeSave;
    private readonly GameServices _services;

    private int _selectedIndex = 0;

    public StatsScreen(DungeonSession session, CampaignConfig config, ActiveSave activeSave, GameServices services)
    {
        _session = session;
        _config = config;
        _activeSave = activeSave;
        _services = services;
    }

    public void OnEnter() { }
    public void OnExit() { }

    public IGameScreen? Update(float dt)
    {
        if (Raylib.IsKeyPressed(KeyboardKey.Escape) ||
            Raylib.IsKeyPressed(KeyboardKey.I))
            return new PlayingScreen(_session, _config, _activeSave, _services);

        var count = _activeSave.Characters.Count;
        if (Raylib.IsKeyPressed(KeyboardKey.Up) || Raylib.IsKeyPressed(KeyboardKey.W))
            _selectedIndex = (_selectedIndex - 1 + count) % count;
        if (Raylib.IsKeyPressed(KeyboardKey.Down) || Raylib.IsKeyPressed(KeyboardKey.S))
            _selectedIndex = (_selectedIndex + 1) % count;

        return null;
    }

    public void Draw(int w, int h)
    {
        var colors = _config.Colors;
        var party = _activeSave.Characters;
        var selected = party.Count > 0 ? party[_selectedIndex] : null;

        // ── Titre ─────────────────────────────────────────────────────────────
        var title = "Équipe";
        var titleSize = FantasyUI.MeasureText(title, 28f);
        FantasyUI.Title(title, (w - titleSize.X) / 2f, 16f, 28f, colors);

        FantasyUI.Label("I / Échap — retour au jeu  ·  ↑↓ — changer de membre",
            0, h - 24f, 13f, colors, colorOverride: colors.TextMuted);

        // ── Layout ────────────────────────────────────────────────────────────
        int panelY = 60;
        int panelH = h - panelY - 36;
        int leftW = 180;

        // ── Panneau gauche — liste ─────────────────────────────────────────────
        DrawPartyList(
            new Rectangle(8, panelY, leftW - 16, panelH),
            party, colors);

        // ── Panneau droit — détail ─────────────────────────────────────────────
        if (selected != null)
            DrawCharacterDetail(
                new Rectangle(leftW, panelY, w - leftW - 8, panelH),
                selected, colors);
    }

    // ── Panneau gauche ────────────────────────────────────────────────────────

    private void DrawPartyList(Rectangle rect, List<Character> party,
                                RaylibColorScheme colors)
    {
        FantasyUI.Panel(rect, colors);

        float cardH = 56f;
        float gap = 8f;
        float y = rect.Y + 10f;

        for (int i = 0; i < party.Count; i++)
        {
            var c = party[i];
            bool sel = i == _selectedIndex;
            var cardR = new Rectangle(rect.X + 8, y, rect.Width - 16, cardH);

            // Sélection par clic
            if (Raylib.CheckCollisionPointRec(Raylib.GetMousePosition(), cardR) &&
                Raylib.IsMouseButtonReleased(MouseButton.Left))
                _selectedIndex = i;

            // Fond carte
            var bg = sel ? colors.Primary : colors.SurfaceSecondary;
            Raylib.DrawRectangleRounded(cardR, 0.2f, 8, bg);
            if (sel)
                Raylib.DrawRectangleRoundedLines(cardR, 0.2f, 8, 2f, colors.Accent);

            // Nom
            FantasyUI.Label(c.Description.Name.Firstname,
                cardR.X + 10, cardR.Y + 8, 16f, colors,
                colorOverride: sel ? colors.Accent : colors.Text);

            // HP
            var hp = $"PV {c.State.CurrentHp}/{c.MaxHp()}";
            var hpCol = HpColor(c.State.CurrentHp, c.MaxHp());
            FantasyUI.Label(hp, cardR.X + 10, cardR.Y + 30, 12f, colors,
                colorOverride: hpCol);

            y += cardH + gap;
        }

        // Placeholder "recruter"
        var recruitR = new Rectangle(rect.X + 8, y, rect.Width - 16, cardH);
        Raylib.DrawRectangleRounded(recruitR, 0.2f, 8, colors.SurfaceSecondary);
        Raylib.DrawRectangleRoundedLines(recruitR, 0.2f, 8, 1f,
            new Color(80, 70, 60, 180));
        var plus = "+ Recruter";
        var plusW = FantasyUI.MeasureText(plus, 14f).X;
        FantasyUI.Label(plus,
            recruitR.X + (recruitR.Width - plusW) / 2f,
            recruitR.Y + (recruitR.Height - 14f) / 2f,
            14f, colors, colorOverride: colors.TextMuted);
    }

    // ── Panneau droit — détail ────────────────────────────────────────────────

    private void DrawCharacterDetail(Rectangle rect, Character c,
                                      RaylibColorScheme colors)
    {
        FantasyUI.Panel(rect, colors);

        float x = rect.X + 20;
        float y = rect.Y + 16;
        float col2 = rect.X + rect.Width / 2f;

        // ── En-tête ───────────────────────────────────────────────────────────
        FantasyUI.Title(c.Description.Name.FullName, x, y, 26f, colors);
        y += 36f;

        var info = $"{c.Description.Gender}  ·  {c.Description.Size}" +
                   $"  ·  {c.Description.Weight}  ·  {c.Description.Sensitivity}";
        FantasyUI.Label(info, x, y, 13f, colors, colorOverride: colors.TextMuted);
        y += 24f;

        if (c.Description.Background != null)
        {
            FantasyUI.Label(c.Description.Background.Name, x, y, 13f, colors,
                colorOverride: colors.TextMuted);
            y += 24f;
        }

        DrawSeparator(rect, y, colors); y += 20f;

        // ── Deux colonnes : Attributs | Combat ────────────────────────────────
        float colY = y;

        // Attributs
        FantasyUI.Label("Attributs", x, colY, 16f, colors,
            colorOverride: colors.Accent);
        colY += 26f;

        DrawStatRow(x, colY, rect.Width / 2f - 20, "Musculature",
            c.Attributes.Musculature.Current(), colors); colY += 22f;
        DrawStatRow(x, colY, rect.Width / 2f - 20, "Flexibilité",
            c.Attributes.Flexibility.Current(), colors); colY += 22f;
        DrawStatRow(x, colY, rect.Width / 2f - 20, "Intelligence",
            c.Attributes.Brain.Current(), colors); colY += 22f;
        DrawStatRow(x, colY, rect.Width / 2f - 20, "Vitalité",
            c.Attributes.Vitality.Current(), colors); colY += 22f;

        // Combat
        float combatY = y;
        FantasyUI.Label("Combat", col2, combatY, 16f, colors,
            colorOverride: colors.Accent);
        combatY += 26f;

        // Barre HP
        DrawHpBar(col2, combatY, rect.Width / 2f - 30,
            c.State.CurrentHp, c.MaxHp(), colors);
        combatY += 30f;

        DrawStatRow(col2, combatY, rect.Width / 2f - 30, "QAm",
            c.FinalMightyAttack(), colors); combatY += 22f;
        DrawStatRow(col2, combatY, rect.Width / 2f - 30, "QAc",
            c.FinalCriticalAttack(), colors); combatY += 22f;
        DrawStatRow(col2, combatY, rect.Width / 2f - 30, "QDp",
            c.FinalParryDefense(), colors); combatY += 22f;
        DrawStatRow(col2, combatY, rect.Width / 2f - 30, "QDe",
            c.FinalDodgeDefense(), colors); combatY += 22f;

        float afterCols = Math.Max(colY, combatY) + 8f;
        DrawSeparator(rect, afterCols, colors);
        afterCols += 20f;

        // ── Compétences ───────────────────────────────────────────────────────
        FantasyUI.Label("Compétences", x, afterCols, 16f, colors,
            colorOverride: colors.Accent);
        afterCols += 26f;

        var skills = c.Skills.All.ToList();
        if (skills.Count == 0)
        {
            FantasyUI.Label("Aucune", x, afterCols, 14f, colors,
                colorOverride: colors.TextMuted);
            afterCols += 22f;
        }
        else
        {
            float sx = x;
            foreach (var skill in skills)
            {
                FantasyUI.Label($"▸ {skill.SkillId}", sx, afterCols, 13f, colors);
                sx += 160f;
                if (sx + 160f > rect.X + rect.Width - 20)
                {
                    sx = x;
                    afterCols += 20f;
                }
            }
            afterCols += 26f;
        }

        DrawSeparator(rect, afterCols, colors);
        afterCols += 20f;

        // ── Blessures ─────────────────────────────────────────────────────────
        FantasyUI.Label("Blessures", x, afterCols, 16f, colors,
            colorOverride: colors.Accent);
        afterCols += 26f;

        if (!c.State.HasInjuries)
        {
            FantasyUI.Label("Aucune", x, afterCols, 14f, colors,
                colorOverride: colors.TextMuted);
        }
        else
        {
            foreach (var injury in c.State.Injuries)
            {
                var desc = FormatInjury(injury);
                var sCol = injury.Severity == InjurySeverity.Severe
                            ? new Color(200, 60, 60, 255)
                            : injury.Severity == InjurySeverity.Moderate
                            ? new Color(220, 150, 50, 255)
                            : colors.TextMuted;
                FantasyUI.Label($"▸ {desc}", x, afterCols, 13f, colors,
                    colorOverride: sCol);
                afterCols += 20f;
            }
        }

        DrawSeparator(rect, afterCols, colors);
        afterCols += 20f;

        // ── Inventaire ────────────────────────────────────────────────────────────
        FantasyUI.Label("Inventaire", x, afterCols, 16f, colors,
            colorOverride: colors.Accent);

        var slotInfo = $"({c.Inventory.SlotCount}/{c.Inventory.MaxSlots ?? 0})";
        var slotW = FantasyUI.MeasureText(slotInfo, 13f).X;
        FantasyUI.Label(slotInfo, rect.X + rect.Width - 20 - slotW,
            afterCols + 2f, 13f, colors, colorOverride: colors.TextMuted);
        afterCols += 26f;

        if (c.Inventory.IsEmpty)
        {
            FantasyUI.Label("Vide", x, afterCols, 14f, colors,
                colorOverride: colors.TextMuted);
        }
        else
        {
            var mouse = Raylib.GetMousePosition();
            var clicked = Raylib.IsMouseButtonReleased(MouseButton.Left);
            float itemW = rect.Width - 40f;

            foreach (var (itemId, qty) in c.Inventory.Items.ToList())
            {
                var def = _services.Items.Get(itemId);
                var name = def?.Title ?? itemId;
                var label = qty > 1 ? $"▸ {name}  ×{qty}  [poser]" : $"▸ {name}  [poser]";
                var itemR = new Rectangle(x, afterCols - 2f, itemW, 20f);
                var hover = Raylib.CheckCollisionPointRec(mouse, itemR);

                if (hover)
                    Raylib.DrawRectangleRounded(itemR, 0.2f, 4,
                        new Color(80, 60, 30, 120));

                FantasyUI.Label(label, x, afterCols, 13f, colors,
                    colorOverride: hover ? colors.Accent : colors.Text);

                if (hover && clicked)
                    DropItem(c, itemId, qty);

                afterCols += 20f;
            }
        }
    }

    private void DropItem(Character c, string itemId, int qty)
    {
        var pos = _session.Party.Position;
        var tile = _session.CurrentMap.Map.GetTile(pos);
        if (tile == null) return;

        c.Inventory.Remove(itemId, qty);
        tile.FloorInventory.Add(itemId, qty);

        // Persister dans WorldState
        var mapId = _session.CurrentMap.Map.Name;
        _activeSave.World.SetTileInventory(mapId, pos.X, pos.Y, tile.FloorInventory);
    }

    // ── Helpers de rendu ──────────────────────────────────────────────────────

    private void DrawStatRow(float x, float y, float w,
                              string label, int value, RaylibColorScheme colors)
    {
        FantasyUI.Label(label, x, y, 14f, colors);
        var valStr = value > 0 ? $"+{value}" : value.ToString();
        var valW = FantasyUI.MeasureText(valStr, 14f).X;
        var col = value > 0 ? new Color(100, 200, 120, 255)
                   : value < 0 ? new Color(200, 90, 90, 255)
                               : colors.TextMuted;
        FantasyUI.Label(valStr, x + w - valW, y, 14f, colors, colorOverride: col);
    }

    private static void DrawHpBar(float x, float y, float w,
                                   int current, int max,
                                   RaylibColorScheme colors)
    {
        float ratio = max > 0 ? (float)current / max : 0f;
        var bgRect = new Rectangle(x, y, w, 16f);
        var fgRect = new Rectangle(x, y, w * ratio, 16f);

        Raylib.DrawRectangleRounded(bgRect, 0.5f, 8, colors.SurfaceSecondary);
        Raylib.DrawRectangleRounded(fgRect, 0.5f, 8, HpColor(current, max));
        Raylib.DrawRectangleRoundedLines(bgRect, 0.5f, 8, 1f, colors.Primary);

        var txt = $"{current} / {max}";
        var txtW = FantasyUI.MeasureText(txt, 12f).X;
        FantasyUI.Label(txt, x + (w - txtW) / 2f, y + 1f, 12f, colors);
    }

    private static void DrawSeparator(Rectangle rect, float y, RaylibColorScheme colors)
    {
        Raylib.DrawLine(
            (int)(rect.X + 12), (int)y,
            (int)(rect.X + rect.Width - 12), (int)y,
            colors.Primary);
    }

    private static string FormatInjury(Injury injury) => injury switch
    {
        Injury.Physical p =>
            $"{p.GetType().Name} ({p.Location}) — {p.Severity}",
        Injury.Mental m =>
            $"{m.GetType().Name} ({m.Effect}) — {m.Severity}",
        Injury.Energy e =>
            $"{e.GetType().Name} ({e.Source}) — {e.Severity}",
        _ => injury.Severity.ToString()
    };

    private static Color HpColor(int current, int max)
    {
        float ratio = max > 0 ? (float)current / max : 0f;
        return ratio > 0.6f ? new Color(80, 180, 80, 255)
             : ratio > 0.3f ? new Color(220, 160, 40, 255)
                            : new Color(190, 50, 50, 255);
    }
}
