using System.IO;
using Raylib_cs;
using System.Numerics;
using DungeonCrawler.Core;
using DungeonCrawler.Core.Characters;
using DungeonCrawler.Core.Entities;
using DungeonCrawler.Core.Models;
using DungeonCrawler.Core.Rendering;

namespace DungeonCrawler.RaylibGame;

public readonly record struct DungeonSquare(
    float LeftBack,    float TopBack,    float RightBack,    float BottomBack,
    float LeftForward, float TopForward, float RightForward, float BottomForward);

public static class DungeonRenderer
{
    // ── Résolution interne de la vue 3D ─────────────────────────────────────
    // Vue carrée : changer ViewSize ici suffit pour tout adapter.
    public const int ViewSize = 720;

    private const float Cx    = ViewSize / 2f;   // 360
    private const float Cy    = ViewSize / 2f;   // 360
    private const float Ratio    = 0.75f;
    private const int   MaxDepth = 5;

    // Couleurs solides (fallback si textures non chargées)
    private static readonly Color WallFront   = new(100, 92, 82, 255);
    private static readonly Color WallSide    = new( 62, 57, 50, 255);
    private static readonly Color DoorFront   = new(140, 88, 28, 255);
    private static readonly Color DoorSide    = new( 88, 55, 18, 255);
    private static readonly Color StairsColor = new( 75, 96, 75, 255);
    private static readonly Color FloorCol    = new( 48, 38, 28, 255);
    private static readonly Color CeilingCol  = new( 26, 26, 42, 255);

    // =========================================================================
    // Textures
    // =========================================================================

    private static Texture2D _texDoorOpen;
    private static Texture2D _texWall;
    private static Texture2D _texFloor;
    private static Texture2D _texCeiling;
    private static Texture2D _texDoor;
    private static bool      _texLoaded;

    // ── Textures items ────────────────────────────────────────────────────────────
    private static readonly Dictionary<string, Texture2D> _itemTextures = new();

    public static void Init(string assetsPath = "Assets")
    {
        _viewTex = Raylib.LoadRenderTexture(ViewSize, ViewSize);
        _fromTex = Raylib.LoadRenderTexture(ViewSize, ViewSize);
        _toTex = Raylib.LoadRenderTexture(ViewSize, ViewSize);
    }

    public static void Unload()
    {
        if (_texLoaded)
        {
            Raylib.UnloadTexture(_texWall);
            Raylib.UnloadTexture(_texFloor);
            Raylib.UnloadTexture(_texCeiling);
            Raylib.UnloadTexture(_texDoor);
            Raylib.UnloadTexture(_texDoorOpen);
            _texLoaded = false;
        }
        Raylib.UnloadRenderTexture(_viewTex);
        Raylib.UnloadRenderTexture(_fromTex);
        Raylib.UnloadRenderTexture(_toTex);

        foreach (var tex in _itemTextures.Values)
            if (tex.Id > 0) Raylib.UnloadTexture(tex);
        _itemTextures.Clear();
    }

    public static void UnloadTextures() => Unload();


    public static void LoadTextureSet(BiomeTextures? textures)
    {
        if (_texLoaded)
        {
            Raylib.UnloadTexture(_texWall);
            Raylib.UnloadTexture(_texFloor);
            Raylib.UnloadTexture(_texCeiling);
            Raylib.UnloadTexture(_texDoor);
            Raylib.UnloadTexture(_texDoorOpen);
            _texLoaded = false;
        }

        if (textures == null) return;

        _texWall = LoadTex(textures.Wall);
        _texFloor = LoadTex(textures.Floor);
        _texCeiling = LoadTex(textures.Ceiling);
        _texDoor = LoadTex(textures.DoorClosed);
        _texDoorOpen = LoadTex(textures.DoorOpen);

        bool ok = _texWall.Id > 0 && _texFloor.Id > 0 &&
                  _texCeiling.Id > 0 && _texDoor.Id > 0 && _texDoorOpen.Id > 0;

        if (!ok)
            Console.Error.WriteLine("[DungeonRenderer] Une ou plusieurs textures n'ont pas pu être chargées.");

        if (ok)
        {
            Raylib.SetTextureFilter(_texWall, TextureFilter.Bilinear);
            Raylib.SetTextureFilter(_texFloor, TextureFilter.Bilinear);
            Raylib.SetTextureFilter(_texCeiling, TextureFilter.Bilinear);
            Raylib.SetTextureFilter(_texDoor, TextureFilter.Bilinear);
            Raylib.SetTextureFilter(_texDoorOpen, TextureFilter.Bilinear);
            Raylib.SetTextureWrap(_texWall, TextureWrap.Repeat);
            Raylib.SetTextureWrap(_texFloor, TextureWrap.Repeat);
            Raylib.SetTextureWrap(_texCeiling, TextureWrap.Repeat);
        }

        _texLoaded = ok;
    }

    public static void LoadItemTextures(ItemRegistry registry)
    {
        foreach (var tex in _itemTextures.Values)
            if (tex.Id > 0) Raylib.UnloadTexture(tex);
        _itemTextures.Clear();

        foreach (var item in registry.All)
        {
            if (string.IsNullOrEmpty(item.SpritePath) || !File.Exists(item.SpritePath))
                continue;

            var tex = Raylib.LoadTexture(item.SpritePath);
            if (tex.Id > 0)
            {
                Raylib.SetTextureFilter(tex, TextureFilter.Point); // pixel art
                _itemTextures[item.Id] = tex;
            }
        }

        Console.WriteLine($"[DungeonRenderer] {_itemTextures.Count} texture(s) item chargée(s).");
    }

    private static Texture2D LoadTex(string? path)
    {
        if (string.IsNullOrEmpty(path) || !File.Exists(path))
        {
            Console.Error.WriteLine($"[DungeonRenderer] Texture introuvable : {path}");
            return default;
        }
        return Raylib.LoadTexture(path);
    }

    // =========================================================================
    // Quad texturé via rlgl  (équivalent du setPolyToPoly Kotlin)
    //
    //  Ordre RL_QUADS : TL → BL → BR → TR  (même que DrawTexturePro interne)
    //  UV : TL(0,0)  TR(1,0)  BR(1,1)  BL(0,1)
    // =========================================================================

    // =========================================================================
    // Surfaces texturées — approche scanline (DrawTexturePro, sans rlgl)
    //
    //  Chaque trapèze est décomposé en lignes horizontales (sol/plafond)
    //  ou colonnes verticales (murs latéraux) de 1px, dessinées avec
    //  DrawTexturePro. Pas de rlgl, pas de problèmes de batch.
    // =========================================================================

    // ── Sol : lignes horizontales, de far (BottomBack) vers near (BottomForward) ─
    private static void DrawFloor(DungeonSquare sq, Color tint)
    {
        if (!_texLoaded)
        {
            FillQuad(new(sq.LeftBack, sq.BottomBack), new(sq.RightBack, sq.BottomBack),
                     new(sq.RightForward, sq.BottomForward), new(sq.LeftForward, sq.BottomForward), tint);
            return;
        }
        int   y0   = (int)sq.BottomBack;
        int   y1   = Math.Min((int)sq.BottomForward, ViewSize);
        float span = sq.BottomForward - sq.BottomBack;
        if (y1 <= y0 || span <= 0) return;

        for (int y = y0; y < y1; y++)
        {
            float t  = (y - sq.BottomBack) / span;          // 0=far  1=near
            float xL = sq.LeftBack  + t * (sq.LeftForward  - sq.LeftBack);
            float xR = sq.RightBack + t * (sq.RightForward - sq.RightBack);
            if (xR <= xL) continue;
            Raylib.DrawTexturePro(_texFloor,
                new Rectangle(0, t * _texFloor.Height, _texFloor.Width, 1),
                new Rectangle(xL, y, xR - xL, 1),
                Vector2.Zero, 0f, tint);
        }
    }

    // ── Plafond : lignes horizontales, de near (TopForward) vers far (TopBack) ──
    private static void DrawCeiling(DungeonSquare sq, Color tint)
    {
        if (!_texLoaded)
        {
            FillQuad(new(sq.LeftForward, sq.TopForward), new(sq.RightForward, sq.TopForward),
                     new(sq.RightBack, sq.TopBack), new(sq.LeftBack, sq.TopBack), tint);
            return;
        }
        int   y0   = Math.Max((int)sq.TopForward, 0);
        int   y1   = (int)sq.TopBack;
        float span = sq.TopBack - sq.TopForward;
        if (y1 <= y0 || span <= 0) return;

        for (int y = y0; y < y1; y++)
        {
            float t  = (y - sq.TopForward) / span;              // 0=near  1=far
            float xL = sq.LeftForward  + t * (sq.LeftBack  - sq.LeftForward);
            float xR = sq.RightForward + t * (sq.RightBack - sq.RightForward);
            if (xR <= xL) continue;
            Raylib.DrawTexturePro(_texCeiling,
                new Rectangle(0, t * _texCeiling.Height, _texCeiling.Width, 1),
                new Rectangle(xL, y, xR - xL, 1),
                Vector2.Zero, 0f, tint);
        }
    }

    // ── Face frontale : rectangle → DrawTexturePro direct ────────────────────
    private static void DrawFrontWall(DungeonSquare sq, Tile tile, Color tint)
    {
        int x = (int)sq.LeftForward,  y = (int)sq.TopForward;
        int w = (int)(sq.RightForward - sq.LeftForward);
        int h = (int)(sq.BottomForward - sq.TopForward);
        if (w <= 0 || h <= 0) return;

        if (_texLoaded)
        {
            var tex = tile.Tag switch
            {
                TileTag.Door => _texDoor,
                TileTag.DoorOpen => _texDoorOpen,
                _ => _texWall
            };
            Raylib.DrawTexturePro(tex,
                new Rectangle(0, 0, tex.Width, tex.Height),
                new Rectangle(x, y, w, h),
                Vector2.Zero, 0f, tint);
        }
        else Raylib.DrawRectangle(x, y, w, h, tint);
    }

    // ── Face droite (couloir gauche) : near=RightForward, far=RightBack ────────
    private static void DrawRightFace(DungeonSquare sq, Color tint)
    {
        if (!_texLoaded)
        {
            FillQuad(new(sq.RightForward, sq.TopForward), new(sq.RightBack, sq.TopBack),
                     new(sq.RightBack, sq.BottomBack), new(sq.RightForward, sq.BottomForward), tint);
            return;
        }
        // span = RightBack - RightForward (positif : far plus à droite que near)
        // Si RightForward < 0 (player level) le scan est clampé à 0 mais t reste juste.
        float xNear = sq.RightForward;
        float xFar  = sq.RightBack;
        float span  = xFar - xNear;
        int x0 = Math.Max(0,           (int)Math.Min(xNear, xFar));
        int x1 = Math.Min(ViewSize, (int)Math.Max(xNear, xFar));
        if (x1 <= x0 || span == 0f) return;

        for (int x = x0; x < x1; x++)
        {
            float t  = (x - xNear) / span;              // 0=near  1=far
            float yT = sq.TopForward    + t * (sq.TopBack    - sq.TopForward);
            float yB = sq.BottomForward + t * (sq.BottomBack - sq.BottomForward);
            float cT = Math.Max(0, yT), cB = Math.Min(ViewSize, yB);
            if (cB <= cT) continue;
            float uvYs = (cT - yT) / (yB - yT);
            float uvYe = (cB - yT) / (yB - yT);
            Raylib.DrawTexturePro(_texWall,
                new Rectangle(t * _texWall.Width, uvYs * _texWall.Height,
                              1, (uvYe - uvYs) * _texWall.Height),
                new Rectangle(x, cT, 1, cB - cT),
                Vector2.Zero, 0f, tint);
        }
    }

    // ── Face gauche (couloir droit) : near=LeftForward, far=LeftBack ──────────
    private static void DrawLeftFace(DungeonSquare sq, Color tint)
    {
        if (!_texLoaded)
        {
            FillQuad(new(sq.LeftBack, sq.TopBack), new(sq.LeftForward, sq.TopForward),
                     new(sq.LeftForward, sq.BottomForward), new(sq.LeftBack, sq.BottomBack), tint);
            return;
        }
        // span = LeftBack - LeftForward (négatif : far plus à gauche que near)
        float xNear = sq.LeftForward;
        float xFar  = sq.LeftBack;
        float span  = xFar - xNear;
        int x0 = Math.Max(0,           (int)Math.Min(xNear, xFar));
        int x1 = Math.Min(ViewSize, (int)Math.Max(xNear, xFar));
        if (x1 <= x0 || span == 0f) return;

        for (int x = x0; x < x1; x++)
        {
            float t  = (x - xNear) / span;              // 0=near  1=far
            float yT = sq.TopForward    + t * (sq.TopBack    - sq.TopForward);
            float yB = sq.BottomForward + t * (sq.BottomBack - sq.BottomForward);
            float cT = Math.Max(0, yT), cB = Math.Min(ViewSize, yB);
            if (cB <= cT) continue;
            float uvYs = (cT - yT) / (yB - yT);
            float uvYe = (cB - yT) / (yB - yT);
            Raylib.DrawTexturePro(_texWall,
                new Rectangle(t * _texWall.Width, uvYs * _texWall.Height,
                              1, (uvYe - uvYs) * _texWall.Height),
                new Rectangle(x, cT, 1, cB - cT),
                Vector2.Zero, 0f, tint);
        }
    }

    private static void DrawFloorItem(DungeonSquare sq, string itemId)
    {
        float fw = sq.RightForward - sq.LeftForward;
        float fh = sq.BottomForward - sq.TopForward;
        if (fw < 4f || fh < 4f) return;

        float pw = Math.Max(6f, fw * 0.20f);
        float ph = pw * 1.6f;
        float px = sq.LeftForward + fw * 0.5f - pw * 0.5f;
        float py = sq.BottomForward - ph - fh * 0.02f;

        if (_itemTextures.TryGetValue(itemId, out var tex) && tex.Id > 0)
            Raylib.DrawTexturePro(tex,
                new Rectangle(0, 0, tex.Width, tex.Height),
                new Rectangle(px, py, pw, ph),
                Vector2.Zero, 0f, Color.White);
        else
            DrawPotionAt((int)px, (int)py, (int)pw, (int)ph);
    }

    private static void DrawCloseFloorItem(string itemId)
    {
        float pw = 44f;
        float ph = pw * 1.6f;
        float px = Cx - pw * 0.5f;
        float py = ViewSize - ph - 24f;

        if (_itemTextures.TryGetValue(itemId, out var tex) && tex.Id > 0)
            Raylib.DrawTexturePro(tex,
                new Rectangle(0, 0, tex.Width, tex.Height),
                new Rectangle(px, py, pw, ph),
                Vector2.Zero, 0f, Color.White);
        else
            DrawPotionAt((int)px, (int)py, (int)pw, (int)ph);
    }

    private static void DrawPotionAt(int px, int py, int pw, int ph)
    {
        if (pw < 4 || ph < 4) return;

        // Bouchon
        Raylib.DrawRectangle(
            px + (int)(pw * 0.2f), py,
            (int)(pw * 0.6f), Math.Max(1, (int)(ph * 0.12f)),
            new Color(74, 48, 16, 255));
        // Col
        Raylib.DrawRectangle(
            px + (int)(pw * 0.2f), py + (int)(ph * 0.12f),
            (int)(pw * 0.6f), Math.Max(1, (int)(ph * 0.18f)),
            new Color(107, 74, 26, 255));
        // Corps
        Raylib.DrawRectangle(
            px, py + (int)(ph * 0.30f),
            pw, (int)(ph * 0.70f),
            new Color(204, 34, 34, 255));
        // Reflet
        if (pw >= 6)
            Raylib.DrawRectangle(
                px + (int)(pw * 0.1f), py + (int)(ph * 0.35f),
                Math.Max(1, (int)(pw * 0.2f)), (int)(ph * 0.45f),
                new Color(255, 100, 100, 120));
        // Ombre
        if (pw >= 8)
            Raylib.DrawRectangle(
                px + (int)(pw * 0.75f), py + (int)(ph * 0.30f),
                Math.Max(1, (int)(pw * 0.25f)), (int)(ph * 0.70f),
                new Color(100, 0, 0, 100));
    }

    // =========================================================================
    // Point d'entrée
    // =========================================================================

    // =========================================================================
    // Animation — RenderTextures
    // =========================================================================

    private static RenderTexture2D _viewTex;
    private static RenderTexture2D _fromTex;
    private static RenderTexture2D _toTex;

    /// Capture la vue AVANT le déplacement (pour l'animation).
    public static void CaptureFrom(DungeonView view, DungeonRunner runner)
        => CaptureInto(_fromTex, view, runner);

    /// Capture la vue APRÈS le déplacement (pour l'animation).
    public static void CaptureTo(DungeonView view, DungeonRunner runner)
        => CaptureInto(_toTex, view, runner);

    private static void CaptureInto(RenderTexture2D rt, DungeonView view, DungeonRunner runner)
    {
        Raylib.BeginTextureMode(rt);
        Raylib.ClearBackground(new Color(6, 5, 4, 255));
        DrawDungeonView(view, runner);
        Raylib.EndTextureMode();
    }

    // =========================================================================
    // Points d'entrée rendu
    // =========================================================================

    /// Rend la vue 3D dans la texture interne (_viewTex 720×720).
    /// Appeler ensuite DrawSceneAt() + DrawHud() dans BeginDrawing/EndDrawing.
    public static void RenderScene(DungeonView view, DungeonRunner runner)
    {
        Raylib.BeginTextureMode(_viewTex);
        Raylib.ClearBackground(new Color(6, 5, 4, 255));
        DrawDungeonView(view, runner);
        Raylib.EndTextureMode();
    }

    /// Affiche la texture interne dans le rectangle dest (scaling automatique).
    public static void DrawSceneAt(Rectangle dest)
    {
        Raylib.DrawTexturePro(_viewTex.Texture,
            new Rectangle(0, 0, ViewSize, -ViewSize),  // -ViewSize = flip Y du RT
            dest, Vector2.Zero, 0f, Color.White);
    }

    /// Blend animé dessiné directement à l'écran dans le rect dest.
    /// À appeler entre BeginDrawing / EndDrawing.
    /// Le scissor empêche le débordement dans les panneaux UI voisins.
    public static void DrawAnimatedSceneAt(AnimType type, float t, Rectangle dest)
    {
        float s = t * t * (3f - 2f * t);   // smoothstep

        // Scissor en coordonnées OpenGL (Y=0 en bas)
        int sy = Raylib.GetScreenHeight() - (int)(dest.Y + dest.Height);
        Rlgl.EnableScissorTest();
        Rlgl.Scissor((int)dest.X, sy, (int)dest.Width, (int)dest.Height);

        switch (type)
        {
            // ── Avancer : from zoome (opaque), to apparaît en dessous ────────
            case AnimType.Forward:
                DrawRtScaledAt(_fromTex, 1f + s * (1f / Ratio - 1f), Color.White, dest);
                DrawRtScaledAt(_toTex,   Ratio + s * (1f - Ratio),   Fade(s),     dest);
                break;

            // ── Reculer : from rétrécit (opaque), to fade in ─────────────────
            case AnimType.Backward:
                DrawRtScaledAt(_fromTex, 1f - s * (1f - Ratio),              Color.White, dest);
                DrawRtScaledAt(_toTex,   1f / Ratio - s * (1f / Ratio - 1f), Fade(s),     dest);
                break;

            // ── Tourner / Strafer gauche ──────────────────────────────────────
            case AnimType.TurnLeft:
            case AnimType.StrafeLeft:
                DrawRtSlideAt(_fromTex,  s * dest.Width,        dest);
                DrawRtSlideAt(_toTex,   (s - 1f) * dest.Width,  dest);
                break;

            // ── Tourner / Strafer droite ──────────────────────────────────────
            case AnimType.TurnRight:
            case AnimType.StrafeRight:
                DrawRtSlideAt(_fromTex, -s * dest.Width,        dest);
                DrawRtSlideAt(_toTex,   (1f - s) * dest.Width,  dest);
                break;
        }

        Rlgl.DisableScissorTest();
    }

    private static void DrawRtScaledAt(RenderTexture2D rt, float scale, Color tint, Rectangle dest)
    {
        float w = dest.Width  * scale;
        float h = dest.Height * scale;
        Raylib.DrawTexturePro(rt.Texture,
            new Rectangle(0, 0, ViewSize, -ViewSize),   // -ViewSize = flip Y (RT→screen)
            new Rectangle(dest.X + (dest.Width  - w) * .5f,
                          dest.Y + (dest.Height - h) * .5f, w, h),
            Vector2.Zero, 0f, tint);
    }

    private static void DrawRtSlideAt(RenderTexture2D rt, float offsetX, Rectangle dest)
    {
        Raylib.DrawTexturePro(rt.Texture,
            new Rectangle(0, 0, ViewSize, -ViewSize),
            new Rectangle(dest.X + offsetX, dest.Y, dest.Width, dest.Height),
            Vector2.Zero, 0f, Color.White);
    }

    // ── Helpers RenderTexture ─────────────────────────────────────────────────

    // Source rect : hauteur négative pour corriger le flip Y du RenderTexture.
    private static readonly Rectangle RtSrc = new(0, 0, ViewSize, -ViewSize);

    private static void DrawRtScaled(RenderTexture2D rt, float scale, Color tint)
    {
        float w = ViewSize * scale;
        float h = ViewSize  * scale;
        Raylib.DrawTexturePro(rt.Texture, RtSrc,
            new Rectangle((ViewSize - w) * .5f, (ViewSize - h) * .5f, w, h),
            Vector2.Zero, 0f, tint);
    }

    private static void DrawRtSlide(RenderTexture2D rt, float offsetX)
    {
        Raylib.DrawTexturePro(rt.Texture, RtSrc,
            new Rectangle(offsetX, 0, ViewSize, ViewSize),
            Vector2.Zero, 0f, Color.White);
    }

    private static Color Fade(float a)
        => new Color((byte)255, (byte)255, (byte)255, (byte)Math.Clamp(a * 255f, 0f, 255f));

    // =========================================================================
    // Vue 3D (sans HUD) — utilisée par Render et CaptureInto
    // =========================================================================

    private static void DrawDungeonView(DungeonView view, DungeonRunner runner)
    {
        var party = runner.Party;
        var map   = runner.Map;

        var cells     = view.Cells.ToDictionary(c => (c.LateralOffset, c.Distance));
        var entityMap = view.VisibleEntities
                            .GroupBy(ve => (ve.LateralOffset, ve.Distance))
                            .ToDictionary(g => g.Key, g => g.First());

        DrawDepth(new Vector2(ViewSize * Ratio, ViewSize * Ratio),
                  MaxDepth - 1, cells, entityMap);

        float fwdW = ViewSize / Ratio;
        float fwdH = ViewSize  / Ratio;
        float bkW  = ViewSize * Ratio;
        float bkH  = ViewSize  * Ratio;

        var playerSq = new DungeonSquare(
            LeftBack:      Cx - bkW  * .5f,
            TopBack:       Cy - bkH  * .5f,
            RightBack:     Cx + bkW  * .5f,
            BottomBack:    Cy + bkH  * .5f,
            LeftForward:   Cx - fwdW * .5f,
            TopForward:    Cy - fwdH * .5f,
            RightForward:  Cx + fwdW * .5f,
            BottomForward: Cy + fwdH * .5f);

        DrawPlayerLevel(playerSq, fwdW, bkW, map, party);
    }

    // =========================================================================
    // Case du joueur + voisins gauche/droite
    // =========================================================================

    private static void DrawPlayerLevel(
        DungeonSquare sq, float oldW, float newW,
        DungeonMap map, Party party)
    {
        var left  = party.Facing.TurnLeft();
        var right = party.Facing.TurnRight();

        // dist=0 → tint blanc (pleine luminosité)
        // Les faces latérales sont légèrement plus sombres
        var tint     = Color.White;
        var sideTint = Raylib.ColorBrightness(Color.White, -0.2f);

        var leftSq = sq;
        for (int i = 1; i <= 4; i++)
        {
            leftSq = Shift(leftSq, -oldW, -newW);
            if (leftSq.RightBack < 0) break;

            var tile = map.GetTile(party.Position + left.ToOffset() * i);
            if (tile == null) break;

            if (tile.IsSolid) DrawRightFace(leftSq, sideTint);
            else { DrawFloor(leftSq, tint); DrawCeiling(leftSq, tint); }
        }

        var rightSq = sq;
        for (int i = 1; i <= 4; i++)
        {
            rightSq = Shift(rightSq, +oldW, +newW);
            if (rightSq.LeftBack > ViewSize) break;

            var tile = map.GetTile(party.Position + right.ToOffset() * i);
            if (tile == null) break;

            if (tile.IsSolid) DrawLeftFace(rightSq, sideTint);
            else { DrawFloor(rightSq, tint); DrawCeiling(rightSq, tint); }
        }

        DrawFloor(sq,   tint);
        DrawCeiling(sq, tint);

        // Items sur la tile du joueur
        var playerTile = map.GetTile(party.Position);
        if (playerTile != null && !playerTile.FloorInventory.IsEmpty)
        {
            var firstId = playerTile.FloorInventory.Items.Keys.First();
            DrawCloseFloorItem(firstId);
        }
    }

    // =========================================================================
    // Récursion profondeur
    // =========================================================================

    private static void DrawDepth(
        Vector2 oldSize, int deep,
        Dictionary<(int, int), VisibleCell>   cells,
        Dictionary<(int, int), VisibleEntity> entities)
    {
        var newSize = oldSize * Ratio;

        var sq = new DungeonSquare(
            LeftBack:     Cx - newSize.X * .5f,  TopBack:       Cy - newSize.Y * .5f,
            RightBack:    Cx + newSize.X * .5f,  BottomBack:    Cy + newSize.Y * .5f,
            LeftForward:  Cx - oldSize.X * .5f,  TopForward:    Cy - oldSize.Y * .5f,
            RightForward: Cx + oldSize.X * .5f,  BottomForward: Cy + oldSize.Y * .5f);

        if (deep > 0) DrawDepth(newSize, deep - 1, cells, entities);

        int dist = MaxDepth - deep;
        DrawSlice(sq, oldSize.X, newSize.X, dist, cells, entities);
    }

    // =========================================================================
    // Tranche (gauche → droite → centre)
    // =========================================================================

    private static void DrawSlice(
        DungeonSquare center, float oldW, float newW, int dist,
        Dictionary<(int, int), VisibleCell>   cells,
        Dictionary<(int, int), VisibleEntity> entities)
    {
        // Accumuler les latéraux, puis dessiner de l'EXTÉRIEUR vers l'INTÉRIEUR.
        // Si on dessine inside-out, lat=-2 peint sa face frontale sur la face
        // droite de lat=-1 (qui a été dessinée avant). En inversant, lat=-1
        // est toujours dessiné en dernier et recouvre correctement lat=-2.
        var left  = new List<(DungeonSquare, int)>();
        var right = new List<(DungeonSquare, int)>();

        var lSq = center;
        for (int lat = -1; lat >= -6; lat--)
        {
            lSq = Shift(lSq, -oldW, -newW);
            if (lSq.RightBack < 0) break;
            left.Add((lSq, lat));
        }
        var rSq = center;
        for (int lat = 1; lat <= 6; lat++)
        {
            rSq = Shift(rSq, +oldW, +newW);
            if (rSq.LeftBack > ViewSize) break;
            right.Add((rSq, lat));
        }

        // Extérieur (fin de liste) → intérieur (lat = ±1, début de liste)
        for (int i = left.Count  - 1; i >= 0; i--)
            DrawCell(left[i].Item1,  left[i].Item2,  dist, isLeft: true,  isRight: false, cells, entities);
        for (int i = right.Count - 1; i >= 0; i--)
            DrawCell(right[i].Item1, right[i].Item2, dist, isLeft: false, isRight: true,  cells, entities);

        DrawCell(center, 0, dist, isLeft: false, isRight: false, cells, entities);
    }

    // =========================================================================
    // Cellule individuelle
    // =========================================================================

    private static void DrawCell(
        DungeonSquare sq, int lat, int dist,
        bool isLeft, bool isRight,
        Dictionary<(int, int), VisibleCell>   cells,
        Dictionary<(int, int), VisibleEntity> entities)
    {
        if (!cells.TryGetValue((lat, dist), out var cell)) return;

        // Assombrissement progressif avec la profondeur
        float dim      = -0.11f * (dist - 1);
        var   tint     = Raylib.ColorBrightness(Color.White, dim);
        var   sideTint = Raylib.ColorBrightness(Color.White, dim - 0.2f);

        if (cell.Tile.IsSolid)
        {
            if (isLeft)       DrawRightFace(sq, sideTint);
            else if (isRight) DrawLeftFace(sq,  sideTint);

            // Face frontale : texture mur ou porte, avec fallback couleur
            var frontTint = _texLoaded ? tint : Raylib.ColorBrightness(FrontColor(cell.Tile), dim);
            DrawFrontWall(sq, cell.Tile, frontTint);
        }
        else if (cell.Tile.Tag == TileTag.DoorOpen)
        {
            // Porte ouverte : on voit à travers + texture porte ouverte en fond
            var floorTint = _texLoaded ? tint : Raylib.ColorBrightness(FloorCol, dim);
            var ceilTint = _texLoaded ? tint : Raylib.ColorBrightness(CeilingCol, dim);
            DrawFloor(sq, floorTint);
            DrawCeiling(sq, ceilTint);
            if (_texLoaded)
            {
                var frontTint = Raylib.ColorBrightness(tint, -0.3f); // légèrement sombre
                DrawFrontWall(sq, cell.Tile, frontTint);
            }
        }
        else
        {
            var floorTint = _texLoaded ? tint : Raylib.ColorBrightness(FloorCol,   dim);
            var ceilTint  = _texLoaded ? tint : Raylib.ColorBrightness(CeilingCol, dim);

            DrawFloor(sq,   floorTint);
            DrawCeiling(sq, ceilTint);

            if (!cell.Tile.FloorInventory.IsEmpty)
            {
                var firstId = cell.Tile.FloorInventory.Items.Keys.First();
                DrawFloorItem(sq, firstId);
            }

            if (entities.TryGetValue((lat, dist), out var ve))
                DrawEntity(sq, ve.Entity);
        }
    }

    // =========================================================================
    // Primitives
    // =========================================================================

    private static void FillQuad(Vector2 tl, Vector2 tr, Vector2 br, Vector2 bl, Color c)
    {
        Raylib.DrawTriangle(tl, tr, bl, c);
        Raylib.DrawTriangle(tr, br, bl, c);
        Raylib.DrawTriangle(tl, bl, tr, c);
        Raylib.DrawTriangle(tr, bl, br, c);
    }

    // =========================================================================
    // Entité
    // =========================================================================

    private static void DrawEntity(DungeonSquare sq, DungeonEntity entity)
    {
        float fw = sq.RightForward - sq.LeftForward;
        float fh = sq.BottomForward - sq.TopForward;
        if (fw < 2 || fh < 2) return;

        (Color color, float wF, float hF) = entity switch
        {
            MonsterEntity => (new Color(210, 45,  45,  255), 0.34f, 0.80f),
            NpcEntity     => (new Color( 45, 185,  70, 255), 0.28f, 0.76f),
            ItemEntity    => (new Color( 65, 205, 225, 255), 0.20f, 0.26f),
            _             => (Color.White,                   0.28f, 0.60f)
        };

        int ew = Math.Max(2, (int)(fw * wF));
        int eh = Math.Max(2, (int)(fh * hF));
        int ex = (int)(sq.LeftForward + fw * .5f) - ew / 2;
        int ey = (int)(sq.BottomForward - fh * .04f) - eh;
        Raylib.DrawRectangle(ex, ey, ew, eh, color);

        string name = entity switch
        {
            MonsterEntity m => m.Name,
            NpcEntity n     => n.Name,
            _ => string.Empty
        };
        if (name.Length > 0)
        {
            int tw = Raylib.MeasureText(name, 11);
            Raylib.DrawText(name, ex + ew / 2 - tw / 2, ey - 15, 11, Color.White);
        }
    }

    // =========================================================================
    // HUD
    // =========================================================================

    public static void DrawHud(DungeonView view, int turnNumber, Party party, Rectangle rect)
    {
        int x = (int)rect.X, y = (int)rect.Y, w = (int)rect.Width, h = (int)rect.Height;
        Raylib.DrawRectangle(x, y, w, h, new Color(10, 8, 6, 255));
        Raylib.DrawLine(x, y, x + w, y, new Color(48, 42, 36, 255));

        Raylib.DrawText(
            $"Turn {turnNumber}   {party.Position}  {party.Facing.ToArrow()}   " +
            string.Join("  ", party.Members),
            x + 12, y + 12, 16, new Color(172, 167, 157, 255));

        string prompt = view.FacingTarget?.Type switch
        {
            InteractionType.Door    => "[F]  Open door",
            InteractionType.Npc  when view.FacingTarget.Entity is NpcEntity  n => $"[F]  Talk to {n.Name}",
            InteractionType.Item when view.FacingTarget.Entity is ItemEntity  i => $"[F]  Pick up {i.DisplayName}",
            InteractionType.StairsDown => "[F]  Descend",
            InteractionType.StairsUp   => "[F]  Ascend",
            _ => string.Empty
        };
        if (prompt.Length > 0)
        {
            int pw = Raylib.MeasureText(prompt, 18);
            Raylib.DrawText(prompt, x + (w - pw) / 2, y + 38, 18, new Color(100, 220, 100, 255));
        }

        const string hint = "W/S Fwd/Back   A/D Turn   Q/E Strafe   F Interact   Esc Quit";
        int hw = Raylib.MeasureText(hint, 12);
        Raylib.DrawText(hint, x + w - hw - 8, y + h - 18, 12, new Color(62, 57, 52, 255));
    }

    // =========================================================================
    // Helpers
    // =========================================================================

    private static DungeonSquare Shift(DungeonSquare sq, float oldW, float newW) => sq with
    {
        LeftBack     = sq.LeftBack     + newW,
        RightBack    = sq.RightBack    + newW,
        LeftForward  = sq.LeftForward  + oldW,
        RightForward = sq.RightForward + oldW
    };

    private static Color FrontColor(Tile t) => t.Tag switch
    {
        TileTag.Door => DoorFront,
        TileTag.DoorOpen => DoorFront,
        TileTag.StairsUp => StairsColor,
        TileTag.StairsDown => StairsColor,
        _ => WallFront
    };

    private static Color SideColor(Tile t) => t.Tag == TileTag.Door ? DoorSide : WallSide;
}
