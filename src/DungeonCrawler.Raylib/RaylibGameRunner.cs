using DungeonCrawler.Core;
using DungeonCrawler.Core.Rendering;
using DungeonCrawler.Core.Systems;
using DungeonCrawler.RaylibGame;
using Raylib_cs;

namespace DungeonCrawler.RaylibGame;

public class RaylibGameRunner
{
    private readonly DungeonRunner _runner;
    private readonly TurnManager _turns;

    private AnimationState _anim = new();
    private DungeonView _currentView = null!;

    public RaylibGameRunner(DungeonRunner runner, TurnManager turns)
    {
        _runner = runner;
        _turns = turns;
    }

    public void Run(string title = "DungeonCrawler", int width = 1100, int height = 760,
                    string assetsPath = "Assets")
    {
        Raylib.SetConfigFlags(ConfigFlags.ResizableWindow);
        Raylib.InitWindow(width, height, title);
        Raylib.SetTargetFPS(60);
        DungeonRenderer.Init(assetsPath);

        _anim = new AnimationState();
        _currentView = _runner.GetView();

        while (!Raylib.WindowShouldClose())
        {
            float dt = Raylib.GetFrameTime();
            _anim.Update(dt);

            if (!_anim.IsPlaying)
                HandleInput();

            if (!_anim.IsPlaying)
            {
                _currentView = _runner.GetView();
                DungeonRenderer.RenderScene(_currentView, _runner);
            }

            Raylib.BeginDrawing();
            Raylib.ClearBackground(new Color(18, 16, 14, 255));

            var layout = ComputeLayout(Raylib.GetScreenWidth(), Raylib.GetScreenHeight());

            if (_anim.IsPlaying)
                DungeonRenderer.DrawAnimatedSceneAt(_anim.Type, _anim.Progress, layout.ViewRect);
            else
                DungeonRenderer.DrawSceneAt(layout.ViewRect);

            DrawUiPanel(layout.UiRect);
            DungeonRenderer.DrawHud(_currentView, _turns.TurnNumber, _runner.Party, layout.HudRect);

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
            _currentView = _runner.GetView();
            DungeonRenderer.CaptureFrom(_currentView, _runner);

            var posBefore = _runner.Party.Position;
            var facingBefore = _runner.Party.Facing;
            _turns.ExecuteAction(action.Value);

            if (_runner.Party.Position != posBefore || _runner.Party.Facing != facingBefore)
            {
                _currentView = _runner.GetView();
                DungeonRenderer.CaptureTo(_currentView, _runner);
                _anim.Start(animType.Value);
            }
            else
            {
                _currentView = _runner.GetView();
            }
        }
        else
        {
            _turns.ExecuteAction(action.Value);
            _currentView = _runner.GetView();
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