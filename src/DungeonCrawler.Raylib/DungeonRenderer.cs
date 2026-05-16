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
    float LeftBack, float TopBack, float RightBack, float BottomBack,
    float LeftForward, float TopForward, float RightForward, float BottomForward);

public static class DungeonRenderer
{
    public const int ScreenWidth = 800;
    public const int ViewHeight = 540;
    public const int HudHeight = 100;
    public const int ScreenHeight = ViewHeight + HudHeight;

    private const float Cx = ScreenWidth / 2f;
    private const float Cy = ViewHeight / 2f;
    private const float Ratio = 0.75f;
    private const int MaxDepth = 5;

    // Couleurs solides (fallback si textures non chargées)
    private static readonly Color WallFront = new(100, 92, 82, 255);
    private static readonly Color WallSide = new(62, 57, 50, 255);
    private static readonly Color DoorFront = new(140, 88, 28, 255);
    private static readonly Color DoorSide = new(88, 55, 18, 255);
    private static readonly Color StairsColor = new(75, 96, 75, 255);
    private static readonly Color FloorCol = new(48, 38, 28, 255);
    private static readonly Color CeilingCol = new(26, 26, 42, 255);

    // =========================================================================
    // Textures
    // =========================================================================

    private static Texture2D _texWall;
    private static Texture2D _texFloor;
    private static Texture2D _texCeiling;
    private static Texture2D _texDoor;
    private static bool _texLoaded;

    public static void LoadTextures(string folder = "Assets")
    {
        _texWall = Raylib.LoadTexture(Path.Combine(folder, "cell_wall.png"));
        _texFloor = Raylib.LoadTexture(Path.Combine(folder, "cell_floor.png"));
        _texCeiling = Raylib.LoadTexture(Path.Combine(folder, "cell_ceiling.png"));
        _texDoor = Raylib.LoadTexture(Path.Combine(folder, "draft_door_closed.png"));

        Raylib.SetTextureFilter(_texWall, TextureFilter.Bilinear);
        Raylib.SetTextureFilter(_texFloor, TextureFilter.Bilinear);
        Raylib.SetTextureFilter(_texCeiling, TextureFilter.Bilinear);
        Raylib.SetTextureFilter(_texDoor, TextureFilter.Bilinear);
        Raylib.SetTextureWrap(_texWall, TextureWrap.Repeat);
        Raylib.SetTextureWrap(_texFloor, TextureWrap.Repeat);
        Raylib.SetTextureWrap(_texCeiling, TextureWrap.Repeat);

        // Validation : raylib retourne Id=0 si le fichier est introuvable
        bool ok = _texWall.Id > 0 && _texFloor.Id > 0 &&
                  _texCeiling.Id > 0 && _texDoor.Id > 0;
        if (!ok)
            Console.Error.WriteLine(
                $"[DungeonRenderer] Texture load failed — wall={_texWall.Id} " +
                $"floor={_texFloor.Id} ceil={_texCeiling.Id} door={_texDoor.Id}\n" +
                $"  Looked in: {Path.GetFullPath(folder)}");
        _texLoaded = ok;
    }

    public static void UnloadTextures()
    {
        if (!_texLoaded) return;
        Raylib.UnloadTexture(_texWall);
        Raylib.UnloadTexture(_texFloor);
        Raylib.UnloadTexture(_texCeiling);
        Raylib.UnloadTexture(_texDoor);
        _texLoaded = false;
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
        int y0 = (int)sq.BottomBack;
        int y1 = Math.Min((int)sq.BottomForward, ViewHeight);
        float span = sq.BottomForward - sq.BottomBack;
        if (y1 <= y0 || span <= 0) return;

        for (int y = y0; y < y1; y++)
        {
            float t = (y - sq.BottomBack) / span;          // 0=far  1=near
            float xL = sq.LeftBack + t * (sq.LeftForward - sq.LeftBack);
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
        int y0 = Math.Max((int)sq.TopForward, 0);
        int y1 = (int)sq.TopBack;
        float span = sq.TopBack - sq.TopForward;
        if (y1 <= y0 || span <= 0) return;

        for (int y = y0; y < y1; y++)
        {
            float t = (y - sq.TopForward) / span;              // 0=near  1=far
            float xL = sq.LeftForward + t * (sq.LeftBack - sq.LeftForward);
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
        int x = (int)sq.LeftForward, y = (int)sq.TopForward;
        int w = (int)(sq.RightForward - sq.LeftForward);
        int h = (int)(sq.BottomForward - sq.TopForward);
        if (w <= 0 || h <= 0) return;

        if (_texLoaded)
        {
            var tex = tile.Tag == TileTag.Door ? _texDoor : _texWall;
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
        float xFar = sq.RightBack;
        float span = xFar - xNear;
        int x0 = Math.Max(0, (int)Math.Min(xNear, xFar));
        int x1 = Math.Min(ScreenWidth, (int)Math.Max(xNear, xFar));
        if (x1 <= x0 || span == 0f) return;

        for (int x = x0; x < x1; x++)
        {
            float t = (x - xNear) / span;              // 0=near  1=far
            float yT = sq.TopForward + t * (sq.TopBack - sq.TopForward);
            float yB = sq.BottomForward + t * (sq.BottomBack - sq.BottomForward);
            float cT = Math.Max(0, yT), cB = Math.Min(ViewHeight, yB);
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
        float xFar = sq.LeftBack;
        float span = xFar - xNear;
        int x0 = Math.Max(0, (int)Math.Min(xNear, xFar));
        int x1 = Math.Min(ScreenWidth, (int)Math.Max(xNear, xFar));
        if (x1 <= x0 || span == 0f) return;

        for (int x = x0; x < x1; x++)
        {
            float t = (x - xNear) / span;              // 0=near  1=far
            float yT = sq.TopForward + t * (sq.TopBack - sq.TopForward);
            float yB = sq.BottomForward + t * (sq.BottomBack - sq.BottomForward);
            float cT = Math.Max(0, yT), cB = Math.Min(ViewHeight, yB);
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

    // =========================================================================
    // Point d'entrée
    // =========================================================================

    // =========================================================================
    // Animation — RenderTextures
    // =========================================================================

    private static RenderTexture2D _fromTex;
    private static RenderTexture2D _toTex;

    public static void InitAnimationTextures()
    {
        _fromTex = Raylib.LoadRenderTexture(ScreenWidth, ViewHeight);
        _toTex = Raylib.LoadRenderTexture(ScreenWidth, ViewHeight);
    }

    public static void UnloadAnimationTextures()
    {
        Raylib.UnloadRenderTexture(_fromTex);
        Raylib.UnloadRenderTexture(_toTex);
    }

    /// Capture la vue AVANT le déplacement.
    public static void CaptureFrom(DungeonView view, DungeonRunner runner)
        => CaptureInto(_fromTex, view, runner);

    /// Capture la vue APRÈS le déplacement.
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

    /// Rendu normal (pas d'animation).
    public static void Render(DungeonView view, DungeonRunner runner, int turnNumber)
    {
        Raylib.BeginDrawing();
        Raylib.ClearBackground(new Color(6, 5, 4, 255));
        DrawDungeonView(view, runner);
        DrawHud(view, turnNumber, runner.Party);
        Raylib.EndDrawing();
    }

    /// Rendu animé : blend entre _fromTex et _toTex selon le type et la progression.
    public static void RenderAnimated(
        AnimType type, float t,
        DungeonView view, DungeonRunner runner, int turnNumber)
    {
        // Smoothstep pour une courbe d'accélération/décélération
        float s = t * t * (3f - 2f * t);

        Raylib.BeginDrawing();
        Raylib.ClearBackground(new Color(6, 5, 4, 255));

        switch (type)
        {
            // ── Avancer : from zoome + disparaît, to apparaît depuis "loin" ──
            case AnimType.Forward:
                DrawRtScaled(_fromTex, 1f + s * (1f / Ratio - 1f), Fade(1f - s));
                DrawRtScaled(_toTex, Ratio + s * (1f - Ratio), Fade(s));
                break;

            // ── Reculer : from rétrécit + disparaît, to arrive depuis "plus près"
            case AnimType.Backward:
                DrawRtScaled(_fromTex, 1f - s * (1f - Ratio), Fade(1f - s));
                DrawRtScaled(_toTex, 1f / Ratio - s * (1f / Ratio - 1f), Fade(s));
                break;

            // ── Tourner / Strafer gauche : from glisse à droite, to arrive de gauche
            case AnimType.TurnLeft:
            case AnimType.StrafeLeft:
                DrawRtSlide(_fromTex, s * ScreenWidth);
                DrawRtSlide(_toTex, (s - 1f) * ScreenWidth);
                break;

            // ── Tourner / Strafer droite : from glisse à gauche, to arrive de droite
            case AnimType.TurnRight:
            case AnimType.StrafeRight:
                DrawRtSlide(_fromTex, -s * ScreenWidth);
                DrawRtSlide(_toTex, (1f - s) * ScreenWidth);
                break;
        }

        DrawHud(view, turnNumber, runner.Party);
        Raylib.EndDrawing();
    }

    // ── Helpers RenderTexture ─────────────────────────────────────────────────

    // Source rect : hauteur négative pour corriger le flip Y du RenderTexture.
    private static readonly Rectangle RtSrc = new(0, 0, ScreenWidth, -ViewHeight);

    private static void DrawRtScaled(RenderTexture2D rt, float scale, Color tint)
    {
        float w = ScreenWidth * scale;
        float h = ViewHeight * scale;
        Raylib.DrawTexturePro(rt.Texture, RtSrc,
            new Rectangle((ScreenWidth - w) * .5f, (ViewHeight - h) * .5f, w, h),
            Vector2.Zero, 0f, tint);
    }

    private static void DrawRtSlide(RenderTexture2D rt, float offsetX)
    {
        Raylib.DrawTexturePro(rt.Texture, RtSrc,
            new Rectangle(offsetX, 0, ScreenWidth, ViewHeight),
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
        var map = runner.Map;

        var cells = view.Cells.ToDictionary(c => (c.LateralOffset, c.Distance));
        var entityMap = view.VisibleEntities
                            .GroupBy(ve => (ve.LateralOffset, ve.Distance))
                            .ToDictionary(g => g.Key, g => g.First());

        DrawDepth(new Vector2(ScreenWidth * Ratio, ViewHeight * Ratio),
                  MaxDepth - 1, cells, entityMap);

        float fwdW = ScreenWidth / Ratio;
        float fwdH = ViewHeight / Ratio;
        float bkW = ScreenWidth * Ratio;
        float bkH = ViewHeight * Ratio;

        var playerSq = new DungeonSquare(
            LeftBack: Cx - bkW * .5f,
            TopBack: Cy - bkH * .5f,
            RightBack: Cx + bkW * .5f,
            BottomBack: Cy + bkH * .5f,
            LeftForward: Cx - fwdW * .5f,
            TopForward: Cy - fwdH * .5f,
            RightForward: Cx + fwdW * .5f,
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
        var left = party.Facing.TurnLeft();
        var right = party.Facing.TurnRight();

        // dist=0 → tint blanc (pleine luminosité)
        // Les faces latérales sont légèrement plus sombres
        var tint = Color.White;
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
            if (rightSq.LeftBack > ScreenWidth) break;

            var tile = map.GetTile(party.Position + right.ToOffset() * i);
            if (tile == null) break;

            if (tile.IsSolid) DrawLeftFace(rightSq, sideTint);
            else { DrawFloor(rightSq, tint); DrawCeiling(rightSq, tint); }
        }

        DrawFloor(sq, tint);
        DrawCeiling(sq, tint);
    }

    // =========================================================================
    // Récursion profondeur
    // =========================================================================

    private static void DrawDepth(
        Vector2 oldSize, int deep,
        Dictionary<(int, int), VisibleCell> cells,
        Dictionary<(int, int), VisibleEntity> entities)
    {
        var newSize = oldSize * Ratio;

        var sq = new DungeonSquare(
            LeftBack: Cx - newSize.X * .5f, TopBack: Cy - newSize.Y * .5f,
            RightBack: Cx + newSize.X * .5f, BottomBack: Cy + newSize.Y * .5f,
            LeftForward: Cx - oldSize.X * .5f, TopForward: Cy - oldSize.Y * .5f,
            RightForward: Cx + oldSize.X * .5f, BottomForward: Cy + oldSize.Y * .5f);

        if (deep > 0) DrawDepth(newSize, deep - 1, cells, entities);

        int dist = MaxDepth - deep;
        DrawSlice(sq, oldSize.X, newSize.X, dist, cells, entities);
    }

    // =========================================================================
    // Tranche (gauche → droite → centre)
    // =========================================================================

    private static void DrawSlice(
        DungeonSquare center, float oldW, float newW, int dist,
        Dictionary<(int, int), VisibleCell> cells,
        Dictionary<(int, int), VisibleEntity> entities)
    {
        // Accumuler les latéraux, puis dessiner de l'EXTÉRIEUR vers l'INTÉRIEUR.
        // Si on dessine inside-out, lat=-2 peint sa face frontale sur la face
        // droite de lat=-1 (qui a été dessinée avant). En inversant, lat=-1
        // est toujours dessiné en dernier et recouvre correctement lat=-2.
        var left = new List<(DungeonSquare, int)>();
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
            if (rSq.LeftBack > ScreenWidth) break;
            right.Add((rSq, lat));
        }

        // Extérieur (fin de liste) → intérieur (lat = ±1, début de liste)
        for (int i = left.Count - 1; i >= 0; i--)
            DrawCell(left[i].Item1, left[i].Item2, dist, isLeft: true, isRight: false, cells, entities);
        for (int i = right.Count - 1; i >= 0; i--)
            DrawCell(right[i].Item1, right[i].Item2, dist, isLeft: false, isRight: true, cells, entities);

        DrawCell(center, 0, dist, isLeft: false, isRight: false, cells, entities);
    }

    // =========================================================================
    // Cellule individuelle
    // =========================================================================

    private static void DrawCell(
        DungeonSquare sq, int lat, int dist,
        bool isLeft, bool isRight,
        Dictionary<(int, int), VisibleCell> cells,
        Dictionary<(int, int), VisibleEntity> entities)
    {
        if (!cells.TryGetValue((lat, dist), out var cell)) return;

        // Assombrissement progressif avec la profondeur
        float dim = -0.11f * (dist - 1);
        var tint = Raylib.ColorBrightness(Color.White, dim);
        var sideTint = Raylib.ColorBrightness(Color.White, dim - 0.2f);

        if (cell.Tile.IsSolid)
        {
            if (isLeft) DrawRightFace(sq, sideTint);
            else if (isRight) DrawLeftFace(sq, sideTint);

            // Face frontale : texture mur ou porte, avec fallback couleur
            var frontTint = _texLoaded ? tint : Raylib.ColorBrightness(FrontColor(cell.Tile), dim);
            DrawFrontWall(sq, cell.Tile, frontTint);
        }
        else
        {
            var floorTint = _texLoaded ? tint : Raylib.ColorBrightness(FloorCol, dim);
            var ceilTint = _texLoaded ? tint : Raylib.ColorBrightness(CeilingCol, dim);

            DrawFloor(sq, floorTint);
            DrawCeiling(sq, ceilTint);

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
            MonsterEntity => (new Color(210, 45, 45, 255), 0.34f, 0.80f),
            NpcEntity => (new Color(45, 185, 70, 255), 0.28f, 0.76f),
            ItemEntity => (new Color(65, 205, 225, 255), 0.20f, 0.26f),
            _ => (Color.White, 0.28f, 0.60f)
        };

        int ew = Math.Max(2, (int)(fw * wF));
        int eh = Math.Max(2, (int)(fh * hF));
        int ex = (int)(sq.LeftForward + fw * .5f) - ew / 2;
        int ey = (int)(sq.BottomForward - fh * .04f) - eh;
        Raylib.DrawRectangle(ex, ey, ew, eh, color);

        string name = entity switch
        {
            MonsterEntity m => m.Name,
            NpcEntity n => n.Name,
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

    private static void DrawHud(DungeonView view, int turnNumber, Party party)
    {
        int y = ViewHeight;
        Raylib.DrawRectangle(0, y, ScreenWidth, HudHeight, new Color(10, 8, 6, 255));
        Raylib.DrawLine(0, y, ScreenWidth, y, new Color(48, 42, 36, 255));

        Raylib.DrawText(
            $"Turn {turnNumber}   {party.Position}  {party.Facing.ToArrow()}   " +
            string.Join("  ", party.Members),
            12, y + 12, 16, new Color(172, 167, 157, 255));

        string prompt = view.FacingTarget?.Type switch
        {
            InteractionType.Door => "[F]  Open door",
            InteractionType.Npc when view.FacingTarget.Entity is NpcEntity n => $"[F]  Talk to {n.Name}",
            InteractionType.Item when view.FacingTarget.Entity is ItemEntity i => $"[F]  Pick up {i.DisplayName}",
            InteractionType.StairsDown => "[F]  Descend",
            InteractionType.StairsUp => "[F]  Ascend",
            _ => string.Empty
        };
        if (prompt.Length > 0)
        {
            int pw = Raylib.MeasureText(prompt, 18);
            Raylib.DrawText(prompt, (ScreenWidth - pw) / 2, y + 38, 18, new Color(100, 220, 100, 255));
        }

        const string hint = "W/S Fwd/Back   A/D Turn   Q/E Strafe   F Interact   Esc Quit";
        int hw = Raylib.MeasureText(hint, 12);
        Raylib.DrawText(hint, ScreenWidth - hw - 8, y + HudHeight - 18, 12, new Color(62, 57, 52, 255));
    }

    // =========================================================================
    // Helpers
    // =========================================================================

    private static DungeonSquare Shift(DungeonSquare sq, float oldW, float newW) => sq with
    {
        LeftBack = sq.LeftBack + newW,
        RightBack = sq.RightBack + newW,
        LeftForward = sq.LeftForward + oldW,
        RightForward = sq.RightForward + oldW
    };

    private static Color FrontColor(Tile t) => t.Tag switch
    {
        TileTag.Door => DoorFront,
        TileTag.StairsUp => StairsColor,
        TileTag.StairsDown => StairsColor,
        _ => WallFront
    };

    private static Color SideColor(Tile t) => t.Tag == TileTag.Door ? DoorSide : WallSide;
}
