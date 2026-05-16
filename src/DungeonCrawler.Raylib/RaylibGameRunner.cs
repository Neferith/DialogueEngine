using DungeonCrawler.Core.Rendering;
using DungeonCrawler.Core.Systems;
using DungeonCrawler.MapLoader;
using DungeonCrawler.RaylibGame;
using Raylib_cs;

namespace DungeonCrawler.RaylibGame;

public class RaylibGameRunner
{
    private readonly DungeonSession _session;

    private AnimationState _anim = new();
    private DungeonView _currentView = null!;

    public RaylibGameRunner(DungeonSession session)
    {
        _session = session;
    }

    public void Run(string title = "DungeonCrawler", int width = 1100, int height = 760,
                    string assetsPath = "Assets")
    {
        Raylib.SetConfigFlags(ConfigFlags.ResizableWindow);
        Raylib.InitWindow(width, height, title);
        Raylib.SetTargetFPS(60);
        DungeonRenderer.Init(assetsPath);

        _anim = new AnimationState();
        _currentView = _session.GetView();

        while (!Raylib.WindowShouldClose())
        {
            float dt = Raylib.GetFrameTime();
            _anim.Update(dt);

            if (!_anim.IsPlaying)
                HandleInput();

            if (!_anim.IsPlaying)
            {
                _currentView = _session.GetView();
                DungeonRenderer.RenderScene(_currentView, _session.Runner);
            }

            Raylib.BeginDrawing();
            Raylib.ClearBackground(new Color(18, 16, 14, 255));

            var layout = ComputeLayout(Raylib.GetScreenWidth(), Raylib.GetScreenHeight());

            if (_anim.IsPlaying)
                DungeonRenderer.DrawAnimatedSceneAt(_anim.Type, _anim.Progress, layout.ViewRect);
            else
                DungeonRenderer.DrawSceneAt(layout.ViewRect);

            DrawUiPanel(layout.UiRect);
            DungeonRenderer.DrawHud(_currentView, _session.TurnNumber,
                                    _session.Party, layout.HudRect);

            Raylib.EndDrawing();
        }

        DungeonRenderer.Unload();
        Raylib.CloseWindow();
    }

    // ── Input ─────────────────────────────────────────────────────────────────

    private void HandleInput()
    {
        PartyActionType? action = null;
        AnimType? animType = null;

        if (Raylib.IsKeyPressed(KeyboardKey.W) || Raylib.IsKeyPressed(KeyboardKey.Up))
            (action, animType) = (PartyActionType.MoveForward, AnimType.Forward);
        else if (Raylib.IsKeyPressed(KeyboardKey.S) || Raylib.IsKeyPressed(KeyboardKey.Down))
            (action, animType) = (PartyActionType.MoveBackward, AnimType.Backward);
        else if (Raylib.IsKeyPressed(KeyboardKey.A) || Raylib.IsKeyPressed(KeyboardKey.Left))
            (action, animType) = (PartyActionType.TurnLeft, AnimType.TurnLeft);
        else if (Raylib.IsKeyPressed(KeyboardKey.D) || Raylib.IsKeyPressed(KeyboardKey.Right))
            (action, animType) = (PartyActionType.TurnRight, AnimType.TurnRight);
        else if (Raylib.IsKeyPressed(KeyboardKey.Q))
            (action, animType) = (PartyActionType.StrafeLeft, AnimType.StrafeLeft);
        else if (Raylib.IsKeyPressed(KeyboardKey.E))
            (action, animType) = (PartyActionType.StrafeRight, AnimType.StrafeRight);
        else if (Raylib.IsKeyPressed(KeyboardKey.F))
            action = PartyActionType.Interact;
        else if (Raylib.IsKeyPressed(KeyboardKey.Space))
            action = PartyActionType.Wait;

        if (!action.HasValue) return;

        if (animType.HasValue)
        {
            _currentView = _session.GetView();
            DungeonRenderer.CaptureFrom(_currentView, _session.Runner);

            var posBefore = _session.Party.Position;
            var facingBefore = _session.Party.Facing;
            _session.ExecuteAction(action.Value);

            if (_session.Party.Position != posBefore || _session.Party.Facing != facingBefore)
            {
                _currentView = _session.GetView();
                DungeonRenderer.CaptureTo(_currentView, _session.Runner);
                _anim.Start(animType.Value);
            }
            else
            {
                _currentView = _session.GetView();
            }
        }
        else
        {
            _session.ExecuteAction(action.Value);
            _currentView = _session.GetView();
        }
    }

    // ── Layout ────────────────────────────────────────────────────────────────

    private static GameLayout ComputeLayout(int winW, int winH)
    {
        const int hudH = 72;
        const int uiMinW = 260;

        int availH = winH - hudH;
        int availW = winW - uiMinW;
        int viewSize = Math.Max(100, Math.Min(availH, availW));

        float viewX = 0;
        float viewY = (winH - hudH - viewSize) / 2f;

        var viewRect = new Rectangle(viewX, viewY, viewSize, viewSize);
        var hudRect = new Rectangle(0, winH - hudH, winW, hudH);
        var uiRect = new Rectangle(viewX + viewSize, 0, winW - viewSize, winH - hudH);

        return new GameLayout(viewRect, uiRect, hudRect);
    }

    // ── UI ────────────────────────────────────────────────────────────────────

    private static void DrawUiPanel(Rectangle rect)
    {
        if (rect.Width <= 0) return;
        Raylib.DrawRectangle((int)rect.X, (int)rect.Y, (int)rect.Width, (int)rect.Height,
                             new Color(22, 20, 18, 255));
        Raylib.DrawLine((int)rect.X, (int)rect.Y, (int)rect.X, (int)(rect.Y + rect.Height),
                        new Color(48, 42, 36, 255));
        Raylib.DrawText("[ UI ]", (int)rect.X + 12, (int)rect.Y + 12, 14,
                        new Color(60, 55, 50, 255));
    }
}