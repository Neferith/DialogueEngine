using Raylib_cs;
using System.Numerics;
using DungeonCrawler.Core;
using DungeonCrawler.Core.Characters;
using DungeonCrawler.Core.Entities;
using DungeonCrawler.Core.Entities.Behaviors;
using DungeonCrawler.Core.Models;
using DungeonCrawler.Core.Systems;
using DungeonCrawler.RaylibGame;

// ── Dungeon setup ─────────────────────────────────────────────────────────────

var map = new DungeonMap(16, 16, "Catacombs - Level 1");
map.FillBorder();
for (int x = 1; x <= 7;  x++) map.SetTile(x, 8, Tile.Wall());
for (int x = 9; x <= 14; x++) map.SetTile(x, 8, Tile.Wall());
for (int y = 10; y <= 13; y++) map.SetTile(4, y, Tile.Wall());
for (int x = 1;  x <= 3;  x++) map.SetTile(x, 10, Tile.Wall());
map.SetTile(4, 12, Tile.Door());
map.SetTile(11, 4, Tile.Wall()); map.SetTile(11, 5, Tile.Wall());
map.SetTile(12, 4, Tile.Wall()); map.SetTile(12, 5, Tile.Wall());
for (int y = 9; y <= 12; y++) map.SetTile(11, y, Tile.Wall());
map.SetTile(13, 13, Tile.StairsDown());

var entities = new EntitySystem();
entities.Add(new MonsterEntity("guard1", new GridPosition(8,  5),  "Skeleton Guard",
             new PatrolBehavior(Direction.North)));
entities.Add(new MonsterEntity("rat1",   new GridPosition(13, 3),  "Giant Rat",
             new AggressiveBehavior(detectionRange: 5)));
entities.Add(new NpcEntity("sage1",      new GridPosition(6,  11), "Old Sage",
             "dialogue_sage", Direction.East));
entities.Add(new ItemEntity("potion1",   new GridPosition(3,  5),  "health_potion", "Health Potion"));

var party = new Party(new GridPosition(4, 4), Direction.North, maxSize: 4);
party.TryAddMember(new PartyMember("Aria"));
party.TryAddMember(new PartyMember("Borin"));

var runner = new DungeonRunner(map, party, entities);
var turns  = new TurnManager(runner, entities);

// ── Window ────────────────────────────────────────────────────────────────────

Raylib.SetConfigFlags(ConfigFlags.ResizableWindow);
Raylib.InitWindow(1100, 760, "DungeonCrawler");
Raylib.SetTargetFPS(60);
DungeonRenderer.Init("Assets");

// ── État ─────────────────────────────────────────────────────────────────────

var  anim        = new AnimationState();
var  currentView = runner.GetView();

// ── Main loop ─────────────────────────────────────────────────────────────────

while (!Raylib.WindowShouldClose())
{
    float dt = Raylib.GetFrameTime();
    anim.Update(dt);

    if (!anim.IsPlaying)
        HandleInput();

    // ── Rendu 3D dans la texture interne (seulement si pas d'animation) ──────
    if (!anim.IsPlaying)
    {
        currentView = runner.GetView();
        DungeonRenderer.RenderScene(currentView, runner);
    }

    // ── Affichage à l'écran ───────────────────────────────────────────────────
    Raylib.BeginDrawing();
    Raylib.ClearBackground(new Color(18, 16, 14, 255));

    var layout = ComputeLayout(Raylib.GetScreenWidth(), Raylib.GetScreenHeight());

    if (anim.IsPlaying)
        DungeonRenderer.DrawAnimatedSceneAt(anim.Type, anim.Progress, layout.ViewRect);
    else
        DungeonRenderer.DrawSceneAt(layout.ViewRect);

    DrawUiPanel(layout.UiRect);
    DungeonRenderer.DrawHud(currentView, turns.TurnNumber, runner.Party, layout.HudRect);

    Raylib.EndDrawing();
}

DungeonRenderer.Unload();
Raylib.CloseWindow();

// ── Layout ────────────────────────────────────────────────────────────────────

GameLayout ComputeLayout(int winW, int winH)
{
    const int hudH    = 72;    // hauteur barre HUD sous la vue
    const int uiMinW  = 260;   // largeur minimale du panneau UI à droite

    // Carré 3D : prend toute la hauteur disponible (moins le HUD), limité par la largeur
    int availH   = winH - hudH;
    int availW   = winW - uiMinW;
    int viewSize = Math.Max(100, Math.Min(availH, availW));

    // Vue centrée verticalement, collée à gauche
    float viewX = 0;
    float viewY = (winH - hudH - viewSize) / 2f;

    var viewRect = new Rectangle(viewX, viewY, viewSize, viewSize);
    var hudRect  = new Rectangle(0,     winH - hudH, winW, hudH);
    var uiRect   = new Rectangle(viewX + viewSize, 0, winW - viewSize, winH - hudH);

    return new GameLayout(viewRect, uiRect, hudRect);
}

// ── Panneau UI (placeholder) ──────────────────────────────────────────────────

void DrawUiPanel(Rectangle rect)
{
    if (rect.Width <= 0) return;
    Raylib.DrawRectangle((int)rect.X, (int)rect.Y, (int)rect.Width, (int)rect.Height,
                         new Color(22, 20, 18, 255));
    Raylib.DrawLine((int)rect.X, (int)rect.Y, (int)rect.X, (int)(rect.Y + rect.Height),
                    new Color(48, 42, 36, 255));
    // Placeholder : "Inventaire / minimap à venir"
    Raylib.DrawText("[ UI ]", (int)rect.X + 12, (int)rect.Y + 12, 14, new Color(60, 55, 50, 255));
}

// ── Input ─────────────────────────────────────────────────────────────────────

void HandleInput()
{
    PartyActionType? action   = null;
    AnimType?        animType = null;

    if      (Raylib.IsKeyPressed(KeyboardKey.W) || Raylib.IsKeyPressed(KeyboardKey.Up))
        (action, animType) = (PartyActionType.MoveForward,  AnimType.Forward);
    else if (Raylib.IsKeyPressed(KeyboardKey.S) || Raylib.IsKeyPressed(KeyboardKey.Down))
        (action, animType) = (PartyActionType.MoveBackward, AnimType.Backward);
    else if (Raylib.IsKeyPressed(KeyboardKey.A) || Raylib.IsKeyPressed(KeyboardKey.Left))
        (action, animType) = (PartyActionType.TurnLeft,     AnimType.TurnLeft);
    else if (Raylib.IsKeyPressed(KeyboardKey.D) || Raylib.IsKeyPressed(KeyboardKey.Right))
        (action, animType) = (PartyActionType.TurnRight,    AnimType.TurnRight);
    else if (Raylib.IsKeyPressed(KeyboardKey.Q))
        (action, animType) = (PartyActionType.StrafeLeft,   AnimType.StrafeLeft);
    else if (Raylib.IsKeyPressed(KeyboardKey.E))
        (action, animType) = (PartyActionType.StrafeRight,  AnimType.StrafeRight);
    else if (Raylib.IsKeyPressed(KeyboardKey.F))
        action = PartyActionType.Interact;
    else if (Raylib.IsKeyPressed(KeyboardKey.Space))
        action = PartyActionType.Wait;

    if (!action.HasValue) return;

    if (animType.HasValue)
    {
        currentView = runner.GetView();
        DungeonRenderer.CaptureFrom(currentView, runner);

        var posBefore    = runner.Party.Position;
        var facingBefore = runner.Party.Facing;
        turns.ExecuteAction(action.Value);

        if (runner.Party.Position != posBefore || runner.Party.Facing != facingBefore)
        {
            currentView = runner.GetView();
            DungeonRenderer.CaptureTo(currentView, runner);
            anim.Start(animType.Value);
        }
        else
        {
            currentView = runner.GetView();
        }
    }
    else
    {
        turns.ExecuteAction(action.Value);
        currentView = runner.GetView();
    }
}
