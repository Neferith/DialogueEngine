using Raylib_cs;

namespace DungeonCrawler.RaylibGame;

public class GameScreenRunner
{
    private readonly CampaignConfig _config;
    private IGameScreen _currentScreen;

    public GameScreenRunner(IGameScreen startScreen, CampaignConfig config)
    {
        _currentScreen = startScreen;
        _config = config;
    }

    public void Run(int width = 1100, int height = 760)
    {
        Raylib.SetConfigFlags(ConfigFlags.ResizableWindow);
        Raylib.InitWindow(width, height, _config.Title);
        Raylib.SetTargetFPS(60);

        _currentScreen.OnEnter();

        while (!Raylib.WindowShouldClose())
        {
            float dt = Raylib.GetFrameTime();
            var next = _currentScreen.Update(dt);

            if (next != null)
            {
                _currentScreen.OnExit();
                _currentScreen = next;
                _currentScreen.OnEnter();
            }

            Raylib.BeginDrawing();
            Raylib.ClearBackground(_config.Colors.Background);
            _currentScreen.Draw(Raylib.GetScreenWidth(), Raylib.GetScreenHeight());
            Raylib.EndDrawing();
        }

        _currentScreen.OnExit();
        Raylib.CloseWindow();
    }
}