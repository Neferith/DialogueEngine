using DungeonCrawler.Core.Systems;
using DungeonCrawler.Core.Rendering;
using DungeonCrawler.MapLoader;
using Raylib_cs;
using DungeonCrawler.Persistence;
using DungeonCrawler.EventSystems;

namespace DungeonCrawler.RaylibGame;

public class PlayingScreen : IGameScreen
{
    private readonly DungeonSession _session;
    private readonly CampaignConfig _config;
    private readonly ActiveSave _activeSave;
    private readonly GameServices _services;

    private readonly Queue<IGameAction> _pendingEffects = new();
    private string? _pendingNotification;
    private float _notificationTimer;

    private readonly DialogueOverlay _dialogueOverlay;
    private readonly PauseOverlay _pauseOverlay;
    private readonly PickupOverlay _pickupOverlay;
    private IGameScreen? _nextScreen;

    private AnimationState _anim = new();
    private DungeonView _currentView = null!;

    public PlayingScreen(DungeonSession session, CampaignConfig config, ActiveSave activeSave, GameServices services)
    {
        _session = session;
        _config = config;
        _activeSave = activeSave;
        _services = services;

        _dialogueOverlay = new DialogueOverlay(_config);
        _pauseOverlay = new PauseOverlay(_config, _activeSave, services);

        _pickupOverlay = new PickupOverlay(_config, _services.Items);
        _pickupOverlay.OnPickup += OnItemPickedUp;
    }

    // ── IGameScreen ───────────────────────────────────────────────────────────

    public void OnEnter()
    {
        DungeonRenderer.Init(_config.AssetsPath);
        DungeonRenderer.LoadTextureSet(_session.CurrentBiomeTextures);
        _session.MapChanged += OnMapChanged;
        _session.EventFired += OnEventFired;

        _anim = new AnimationState();
        _currentView = _session.GetView();

        DungeonRenderer.LoadItemTextures(_services.Items);
        _session.NotifyMapEntered();
    }

    public IGameScreen? Update(float dt)
    {
        _anim.Update(dt);

        // Notification timer
        if (_notificationTimer > 0)
            _notificationTimer -= dt;

        _dialogueOverlay.Update(dt);

        _pauseOverlay.Update();

        _pickupOverlay.Update();

        if (_pauseOverlay.SaveRequested)
            QuickSave();

        if (_pauseOverlay.NextScreen != null)
        {
            _nextScreen = _pauseOverlay.NextScreen;
            _pauseOverlay.ClearNextScreen();
        }

        bool paused = _pauseOverlay.IsActive;
        bool pickupActive = _pickupOverlay.IsActive;

        bool dialogueBlocking = _dialogueOverlay.IsActive && _dialogueOverlay.BlocksInput;

        if (!_anim.IsPlaying && !dialogueBlocking && !paused && !pickupActive)
        {
            HandleInput();
            ProcessPendingEffects();
        }

        if (!_anim.IsPlaying)
        {
            _currentView = _session.GetView();
            DungeonRenderer.RenderScene(_currentView, _session.Runner);
        }

        var next = _nextScreen;
        _nextScreen = null;
        return next;
    }


    private void ProcessPendingEffects()
    {
        while (_pendingEffects.Count > 0)
        {
            var action = _pendingEffects.Dequeue();

            switch (action)
            {
                case ShowMessageAction msg:
                    ShowNotification(msg.Message);
                    break;
                case StartDialogueAction dlg:
                    _dialogueOverlay.Start(dlg.DialogueId);
                    break;
                case GiveItemAction item:
                    // TODO : ajouter à l'inventaire
                    ShowNotification($"Vous obtenez : {item.ItemId} ×{item.Qty}");
                    break;
                case StartCombatAction combat:
                    // TODO : lancer écran de combat
                    ShowNotification($"[Combat] {combat.EncounterId}");
                    break;
                case RemoveEntityAction remove:
                    // TODO : supprimer l'entité
                    break;
                case RecruitAction recruit:
                    // TODO : recrutement
                    ShowNotification($"[Recrutement] {recruit.CharacterId}");
                    break;
            }
        }
    }

    private void ShowNotification(string message)
    {
        _pendingNotification = message;
        _notificationTimer = 3f;
    }

    public void Draw(int screenWidth, int screenHeight)
    {
        var layout = ComputeLayout(screenWidth, screenHeight);

        if (_anim.IsPlaying)
            DungeonRenderer.DrawAnimatedSceneAt(_anim.Type, _anim.Progress, layout.ViewRect);
        else
            DungeonRenderer.DrawSceneAt(layout.ViewRect);

        DrawUiPanel(layout.UiRect);
        DungeonRenderer.DrawHud(_currentView, _session.TurnNumber,
                                _session.Party, layout.HudRect);

        _dialogueOverlay.Draw(screenWidth, screenHeight);
        _pauseOverlay.Draw(screenWidth, screenHeight);
        _pickupOverlay.Draw(screenWidth, screenHeight);

        DrawNotification(screenWidth, screenHeight);
    }

    private void DrawNotification(int w, int h)
    {
        if (_pendingNotification == null || _notificationTimer <= 0) return;

        var colors = _config.Colors;
        float alpha = Math.Min(1f, _notificationTimer);
        var bg = new Color(0, 0, 0, (int)(180 * alpha));
        var rect = new Rectangle(w * 0.2f, h * 0.75f, w * 0.6f, 48f);

        Raylib.DrawRectangleRounded(rect, 0.3f, 8, bg);
        var msgW = FantasyUI.MeasureText(_pendingNotification, 18f).X;
        FantasyUI.Label(_pendingNotification,
            rect.X + (rect.Width - msgW) / 2f,
            rect.Y + 12f,
            18f, colors);
    }

    public void OnExit()
    {
        _session.MapChanged -= OnMapChanged;
        _session.EventFired -= OnEventFired;
        DungeonRenderer.Unload();
    }

    // ── Handlers ──────────────────────────────────────────────────────────────

    private void OnMapChanged(DungeonCrawler.Core.Models.BiomeTextures? textures)
        => DungeonRenderer.LoadTextureSet(textures);

    private void OnEventFired(GameEvent ev, IReadOnlyList<IGameAction> actions)
    {
        foreach (var action in actions)
            _pendingEffects.Enqueue(action);
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
        else if (Raylib.IsKeyPressed(KeyboardKey.I))
            _nextScreen = new StatsScreen(_session, _config, _activeSave, _services);
        else if (Raylib.IsKeyPressed(KeyboardKey.G))
        {
            var tile = _session.CurrentMap.Map.GetTile(_session.Party.Position);
            if (tile != null && !tile.FloorInventory.IsEmpty)
                _pickupOverlay.Open(tile.FloorInventory);
        }
        else if (Raylib.IsKeyPressed(KeyboardKey.F5))
            QuickSave();
        else if (Raylib.IsKeyPressed(KeyboardKey.Escape))
            _pauseOverlay.Toggle();

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

    private bool OnItemPickedUp(string itemId, int qty)
    {
        var hero = _activeSave.Characters.FirstOrDefault();
        if (hero == null) return false;

        if (!hero.Inventory.Add(itemId, qty))
        {
            ShowNotification("Inventaire plein !");
            return false;
        }

        var def = _services.Items.Get(itemId);
        var name = def?.Title ?? itemId;
        ShowNotification($"{name} ramassé.");

        // Persister dans WorldState — la tile est déjà modifiée avant cet appel
        var pos = _session.Party.Position;
        var mapId = _session.CurrentMap.Map.Name;
        var tile = _session.CurrentMap.Map.GetTile(pos);
        Console.WriteLine($"[Pickup] Tile inventory avant SetTileInventory : {string.Join(", ", tile?.FloorInventory.Items.Select(kv => $"{kv.Key}×{kv.Value}") ?? [])}");
        if (tile != null) { 
            _activeSave.World.SetTileInventory(mapId, pos.X, pos.Y, tile.FloorInventory);
            Console.WriteLine($"[Pickup] Override sauvegardé pour {mapId} ({pos.X},{pos.Y})");
        }

        return true;
    }

    private void QuickSave()
    {
        var party = _session.Party;
        var save = new SaveFile
        {
            SlotName = $"Slot {_activeSave.SlotIndex + 1}",
            HeroName = _activeSave.HeroName,
            Location = new LocationSave
            {
                MapId = _session.CurrentMap.Map.Name,
                X = party.Position.X,
                Y = party.Position.Y,
                Facing = party.Facing.ToString().ToUpperInvariant()
            },
            WorldState = _activeSave.World
        };

        foreach (var c in _activeSave.Characters)
            save.Party.Add(CharacterMapper.ToSaveData(c));

        _activeSave.Manager.Save(_activeSave.SlotIndex, save);
        ShowNotification("Partie sauvegardée.");
        Console.WriteLine($"[Save] {_activeSave.HeroName} — {save.Location.MapId} ({party.Position.X},{party.Position.Y})");
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