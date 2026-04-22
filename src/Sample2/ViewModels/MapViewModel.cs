using CommunityToolkit.Mvvm.ComponentModel;
using DialogueEngine.Core.Engine;
using DialogueEngine.Core.Models;
using DialogueEngine.Serialization;
using Avalonia.Platform;
using Sample2.GameState;

namespace Sample2.ViewModels;

public sealed partial class MapViewModel : ObservableObject
{
    // ── Constantes carte ──────────────────────────────────────────────────

    public const double MapW = 640, MapH = 360;
    public const double RoomX1 = 30, RoomY1 = 20, RoomX2 = 610, RoomY2 = 330;
    public const double SpriteW = 28, SpriteH = 32;
    public const double InteractRadius = 65;

    // Positions NPC (centre)
    public const double OfficerCX = 230, OfficerCY = 175;
    public const double GuardCX   = 490, GuardCY   = 175;
    public const double DoorX     = 580, DoorY1    = 110, DoorY2 = 240;

    // Partitions
    public const double WallX     = 380;

    private readonly GameState.GameState _state;
    private readonly GameContext         _ctx;
    private readonly ScriptRegistry      _scripts;

    // ── Joueur ────────────────────────────────────────────────────────────

    [ObservableProperty] private double _playerX = 80 - SpriteW / 2;
    [ObservableProperty] private double _playerY = OfficerCY - SpriteH / 2;

    private double PlayerCX => PlayerX + SpriteW / 2;
    private double PlayerCY => PlayerY + SpriteH / 2;

    // ── NPC computed positions ────────────────────────────────────────────

    public double OfficerLeft => OfficerCX - SpriteW / 2;
    public double OfficerTop  => OfficerCY - SpriteH / 2;
    public double GuardLeft   => GuardCX   - SpriteW / 2;
    public double GuardTop    => GuardCY   - SpriteH / 2;

    // ── Interaction ───────────────────────────────────────────────────────

    [ObservableProperty] private bool _canInteractOfficer;
    [ObservableProperty] private bool _canInteractGuard;

    // ── Dialogue overlay ──────────────────────────────────────────────────

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsDialogueActive))]
    private DialogueViewModel? _activeDialogue;

    public bool IsDialogueActive => ActiveDialogue is not null;

    // ── Porte ─────────────────────────────────────────────────────────────

    [ObservableProperty] private bool _doorOpen;
    [ObservableProperty] private string _statusText = string.Empty;

    public event Action? GameCompleted;

    public MapViewModel(GameState.GameState state)
    {
        _state   = state;
        _ctx     = new GameContext(state);
        _scripts = GameScripts.CreateRegistry(state);
        UpdateStatus();
    }

    // ── Game loop ─────────────────────────────────────────────────────────

    public void Update(bool left, bool right, bool up, bool down)
    {
        if (IsDialogueActive) return;

        const double speed = 3.2;
        double nx = PlayerX, ny = PlayerY;

        if (left)  nx -= speed;
        if (right) nx += speed;
        if (up)    ny -= speed;
        if (down)  ny += speed;

        // Clamp dans la pièce
        nx = Math.Clamp(nx, RoomX1, RoomX2 - SpriteW);
        ny = Math.Clamp(ny, RoomY1, RoomY2 - SpriteH);

        // Mur de partition (sauf dans l'ouverture de la porte)
        bool inDoorGap = (ny + SpriteH / 2) >= DoorY1 && (ny + SpriteH / 2) <= DoorY2;
        if (!inDoorGap)
        {
            bool wasLeft  = PlayerX + SpriteW <= WallX;
            bool willLeft = nx     + SpriteW <= WallX;
            bool wasRight = PlayerX >= WallX;
            bool willRight= nx     >= WallX;
            if (wasLeft  && !willLeft)  nx = WallX - SpriteW;
            if (wasRight && !willRight) nx = WallX;
        }

        PlayerX = nx;
        PlayerY = ny;

        // Mise à jour hints
        CanInteractOfficer = Dist(PlayerCX, PlayerCY, OfficerCX, OfficerCY) < InteractRadius;
        CanInteractGuard   = Dist(PlayerCX, PlayerCY, GuardCX,   GuardCY)   < InteractRadius;
    }

    public void TryInteract()
    {
        if (IsDialogueActive) return;

        if (CanInteractOfficer)
        {
            var file  = _state.OfficerPassGiven ? LoadDialogue("dialogue_officer_post.json")
                                                 : LoadDialogue("dialogue_officer_initial.json");
            var label = _state.OfficerPassGiven
                ? GetPostLabel()
                : "LIEUTENANT MARC";
            StartDialogue(file, label);
        }
        else if (CanInteractGuard)
        {
            StartDialogue(LoadDialogue("dialogue_guard.json"), "GARDE — PORTE EST");
        }
    }

    private void StartDialogue(DialogueFile file, string speaker)
    {
        var vm = new DialogueViewModel(file, _ctx, _scripts, speaker);
        vm.DialogueEnded += () =>
        {
            ActiveDialogue = null;
            DoorOpen = _state.DoorOpen;
            UpdateStatus();
            if (_state.DoorOpen) GameCompleted?.Invoke();
        };
        ActiveDialogue = vm;
    }

    private string GetPostLabel() => _state.OfficerEmotion switch
    {
        OfficerEmotion.Charmed   => "LIEUTENANT MARC  ♥",
        OfficerEmotion.Scared    => "LIEUTENANT MARC  !!",
        OfficerEmotion.Convinced => "LIEUTENANT MARC  ✓",
        _                        => "LIEUTENANT MARC"
    };

    private void UpdateStatus()
    {
        StatusText = _state.HasPass ? "  [LAISSEZ-PASSER : OUI]" : "  [LAISSEZ-PASSER : NON]";
    }

    private static double Dist(double ax, double ay, double bx, double by)
        => Math.Sqrt((ax - bx) * (ax - bx) + (ay - by) * (ay - by));

    private static DialogueFile LoadDialogue(string filename)
    {
        var uri = new Uri($"avares://Sample2/Assets/{filename}");
        using var stream = AssetLoader.Open(uri);
        using var reader = new System.IO.StreamReader(stream);
        return DialogueFileSerializer.Deserialize(reader.ReadToEnd());
    }
}
