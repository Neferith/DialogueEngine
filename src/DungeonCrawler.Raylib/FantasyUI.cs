using System.Numerics;
using Raylib_cs;

namespace DungeonCrawler.RaylibGame;

/// <summary>
/// Helpers de rendu UI dans le style dark fantasy.
/// Appeler Init() après InitWindow(), Unload() avant CloseWindow().
/// </summary>
public static class FantasyUI
{
    private static Font _font;
    private static bool _loaded;

    // ── Init / Unload ─────────────────────────────────────────────────────────

    public static void Init(CampaignConfig config)
    {
        if (string.IsNullOrEmpty(config.FontPath) || !File.Exists(config.FontPath))
        {
            Console.Error.WriteLine($"[FantasyUI] Police introuvable : {config.FontPath}");
            return;
        }
        // Charger à haute résolution pour garder la qualité à toutes les tailles
        _font = Raylib.LoadFontEx(config.FontPath, 256, null, 0);
        Raylib.SetTextureFilter(_font.Texture, TextureFilter.Bilinear);
        _loaded = true;
    }

    public static void Unload()
    {
        if (_loaded) { Raylib.UnloadFont(_font); _loaded = false; }
    }

    // ── Composants ────────────────────────────────────────────────────────────

    /// <summary>Panneau de fond avec bordure.</summary>
    public static void Panel(Rectangle rect, RaylibColorScheme colors)
    {
        // Ombre
        Raylib.DrawRectangleRounded(
            new Rectangle(rect.X + 4, rect.Y + 4, rect.Width, rect.Height),
            0.1f, 8, new Color(0, 0, 0, 100));
        // Fond
        Raylib.DrawRectangleRounded(rect, 0.1f, 8, colors.Surface);
        // Bordure
        Raylib.DrawRectangleRoundedLines(rect, 0.1f, 8, 1f, colors.Primary);
    }

    /// <summary>Bouton — retourne true si cliqué.</summary>
    public static bool Button(Rectangle rect, string text, RaylibColorScheme colors,
                               float fontSize = 20f)
    {
        var mouse = Raylib.GetMousePosition();
        bool hovered = Raylib.CheckCollisionPointRec(mouse, rect);
        bool clicked = hovered && Raylib.IsMouseButtonReleased(MouseButton.Left);

        // Ombre
        Raylib.DrawRectangleRounded(
            new Rectangle(rect.X + 3, rect.Y + 3, rect.Width, rect.Height),
            0.3f, 8, new Color(0, 0, 0, 120));

        // Fond (plus clair au survol)
        var bg = hovered ? Brighten(colors.Primary, 30) : colors.Primary;
        Raylib.DrawRectangleRounded(rect, 0.3f, 8, bg);

        // Bordure or
        var border = hovered ? colors.Accent : Darken(colors.Accent, 40);
        Raylib.DrawRectangleRoundedLines(rect, 0.3f, 8, 2f, border);

        // Texte centré
        DrawCentered(text, rect, fontSize, colors.Text);

        return clicked;
    }

    /// <summary>Titre avec la police médiévale.</summary>
    public static void Title(string text, float x, float y, float fontSize,
                              RaylibColorScheme colors)
        => DrawText(text, new Vector2(x, y), fontSize, 2f, colors.Accent);

    /// <summary>Texte standard.</summary>
    public static void Label(string text, float x, float y, float fontSize,
                              RaylibColorScheme colors, Color? colorOverride = null)
        => DrawText(text, new Vector2(x, y), fontSize, 1f,
                    colorOverride ?? colors.Text);

    /// <summary>
    /// Champ de saisie — affiche la valeur courante.
    /// Le caller gère la logique d'input (voir HandleTextInput).
    /// </summary>
    public static void TextInput(Rectangle rect, string label, string value,
                                  bool isFocused, RaylibColorScheme colors,
                                  float fontSize = 20f)
    {
        // Bordure (or si focus, brun sinon)
        var border = isFocused ? colors.Accent : colors.Primary;
        Raylib.DrawRectangleRounded(rect, 0.2f, 8, colors.SurfaceSecondary);
        Raylib.DrawRectangleRoundedLines(rect, 0.2f, 8, 2f, border);

        // Label au-dessus
        Label(label, rect.X + 4, rect.Y - fontSize - 4, fontSize * 0.8f, colors,
              colorOverride: colors.TextMuted);

        // Valeur + curseur clignotant
        var cursor = isFocused && (int)(Raylib.GetTime() * 2) % 2 == 0 ? "|" : "";
        DrawText(value + cursor,
                 new Vector2(rect.X + 12, rect.Y + (rect.Height - fontSize) / 2f),
                 fontSize, 1f, colors.Text);
    }

    /// <summary>Carte sélectionnable. Retourne true si cliquée.</summary>
    public static bool SelectableCard(Rectangle rect, string title, string? subtitle,
                                       bool isSelected, RaylibColorScheme colors)
    {
        var mouse = Raylib.GetMousePosition();
        bool hovered = Raylib.CheckCollisionPointRec(mouse, rect);
        bool clicked = hovered && Raylib.IsMouseButtonReleased(MouseButton.Left);

        // Fond
        var bg = isSelected ? colors.Primary
               : hovered ? Brighten(colors.Surface, 15)
                            : colors.Surface;
        Raylib.DrawRectangleRounded(rect, 0.15f, 8, bg);

        // Bordure
        var border = isSelected ? colors.Accent
                   : hovered ? colors.Primary
                                : Darken(colors.Surface, 20);
        Raylib.DrawRectangleRoundedLines(rect, 0.15f, 8,
            isSelected ? 2f : 1f, border);

        // Titre centré
        float titleY = subtitle != null
            ? rect.Y + rect.Height * 0.28f
            : rect.Y + (rect.Height - 18f) / 2f;

        var titleMeasure = MeasureText(title, 18f);
        DrawText(title,
            new Vector2(rect.X + (rect.Width - titleMeasure.X) / 2f, titleY),
            18f, 1f, isSelected ? colors.Accent : colors.Text);

        // Sous-titre
        if (subtitle != null)
        {
            var subMeasure = MeasureText(subtitle, 12f);
            DrawText(subtitle,
                new Vector2(rect.X + (rect.Width - subMeasure.X) / 2f,
                            rect.Y + rect.Height * 0.58f),
                12f, 1f, colors.TextMuted);
        }

        return clicked;
    }

    /// <summary>
    /// Gère la saisie clavier pour un champ texte.
    /// Appeler chaque frame quand le champ est focus.
    /// Retourne la nouvelle valeur.
    /// </summary>
    public static string HandleTextInput(string current, int maxLength = 24)
    {
        // Effacement
        if (Raylib.IsKeyPressed(KeyboardKey.Backspace) && current.Length > 0)
            return current[..^1];

        // Caractères
        int ch;
        while ((ch = Raylib.GetCharPressed()) > 0)
        {
            if (current.Length < maxLength && ch >= 32)
                current += (char)ch;
        }

        return current;
    }

    // ── Mesure ────────────────────────────────────────────────────────────────

    public static Vector2 MeasureText(string text, float fontSize) =>
        _loaded
            ? Raylib.MeasureTextEx(_font, text, fontSize, 1f)
            : new Vector2(Raylib.MeasureText(text, (int)fontSize), fontSize);

    // ── Helpers internes ──────────────────────────────────────────────────────

    private static void DrawText(string text, Vector2 pos, float fontSize,
                                  float spacing, Color color)
    {
        if (_loaded)
            Raylib.DrawTextEx(_font, text, pos, fontSize, spacing, color);
        else
            Raylib.DrawText(text, (int)pos.X, (int)pos.Y, (int)fontSize, color);
    }

    private static void DrawCentered(string text, Rectangle rect, float fontSize, Color color)
    {
        var size = MeasureText(text, fontSize);
        var pos = new Vector2(
            rect.X + (rect.Width - size.X) / 2f,
            rect.Y + (rect.Height - size.Y) / 2f);
        DrawText(text, pos, fontSize, 1f, color);
    }

    private static Color Brighten(Color c, int v) =>
        new(Clamp(c.R + v), Clamp(c.G + v), Clamp(c.B + v), c.A);

    private static Color Darken(Color c, int v) =>
        new(Clamp(c.R - v), Clamp(c.G - v), Clamp(c.B - v), c.A);

    private static byte Clamp(int v) => (byte)Math.Clamp(v, 0, 255);
}