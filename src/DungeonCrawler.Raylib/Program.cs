using Raylib_cs;
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

Raylib.InitWindow(DungeonRenderer.ScreenWidth, DungeonRenderer.ScreenHeight, "DungeonCrawler");
Raylib.SetTargetFPS(60);
DungeonRenderer.LoadTextures("Assets");
DungeonRenderer.InitAnimationTextures();

// ── État animation ────────────────────────────────────────────────────────────

var  anim        = new AnimationState();
var  currentView = runner.GetView();

// ── Main loop ─────────────────────────────────────────────────────────────────

while (!Raylib.WindowShouldClose())
{
    float dt = Raylib.GetFrameTime();
    anim.Update(dt);

    if (!anim.IsPlaying)
        HandleInput();

    if (anim.IsPlaying)
        DungeonRenderer.RenderAnimated(anim.Type, anim.Progress, currentView, runner, turns.TurnNumber);
    else
    {
        currentView = runner.GetView();
        DungeonRenderer.Render(currentView, runner, turns.TurnNumber);
    }
}

DungeonRenderer.UnloadAnimationTextures();
DungeonRenderer.UnloadTextures();
Raylib.CloseWindow();

// ── Input ─────────────────────────────────────────────────────────────────────

void HandleInput()
{
    PartyActionType? action    = null;
    AnimType?        animType  = null;

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
        // Capture l'état AVANT le mouvement
        currentView = runner.GetView();
        DungeonRenderer.CaptureFrom(currentView, runner);

        // Mémorise position + facing pour savoir si ça a bougé
        var posBefore    = runner.Party.Position;
        var facingBefore = runner.Party.Facing;

        turns.ExecuteAction(action.Value);

        bool moved = runner.Party.Position != posBefore
                  || runner.Party.Facing   != facingBefore;

        if (moved)
        {
            // Capture l'état APRÈS, démarre l'animation
            currentView = runner.GetView();
            DungeonRenderer.CaptureTo(currentView, runner);
            anim.Start(animType.Value);
        }
        else
        {
            // Mouvement bloqué (mur) : pas d'animation
            currentView = runner.GetView();
        }
    }
    else
    {
        // Interact / Wait : pas d'animation
        turns.ExecuteAction(action.Value);
        currentView = runner.GetView();
    }
}
