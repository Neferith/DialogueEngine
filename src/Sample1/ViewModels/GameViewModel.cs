using Avalonia.Platform;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DialogueEngine.Core.Models;
using DialogueEngine.Serialization;

namespace Sample1.ViewModels;

public sealed partial class GameViewModel : ObservableObject
{
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsSetup), nameof(IsDialogue), nameof(IsOutcome))]
    private object? _currentPhase;

    public bool IsSetup    => _currentPhase is SetupViewModel;
    public bool IsDialogue => _currentPhase is DialogueViewModel;
    public bool IsOutcome  => _currentPhase is OutcomeViewModel;

    private GameState.GameState? _state;

    public GameViewModel()
    {
        var setup = new SetupViewModel();
        setup.GameStarted += StartGame;
        CurrentPhase = setup;
    }

    private void StartGame(GameState.GameState state)
    {
        _state = state;

        var file     = LoadDialogue();
        var context  = new GameContext(state);
        var scripts  = GameScripts.CreateRegistry(state);
        var dialogVm = new DialogueViewModel(file, context, scripts);

        dialogVm.DialogueEnded += () => ShowOutcome(state);

        CurrentPhase = dialogVm;
    }

    private void ShowOutcome(GameState.GameState state)
    {
        CurrentPhase = new OutcomeViewModel(state, RestartGame);
    }

    private void RestartGame()
    {
        var setup = new SetupViewModel();
        setup.GameStarted += StartGame;
        CurrentPhase = setup;
    }

    private static DialogueFile LoadDialogue()
    {
        var uri = new Uri("avares://Sample1/Assets/dialogue_vance.json");
        using var stream = AssetLoader.Open(uri);
        using var reader = new StreamReader(stream);
        var json = reader.ReadToEnd();
        return DialogueFileSerializer.Deserialize(json);
    }
}

// ─────────────────────────────────────────────────────────────────────────────

public sealed class OutcomeViewModel
{
    private readonly Action _restart;

    public string Title   { get; }
    public string Message { get; }

    public OutcomeViewModel(GameState.GameState state, Action restart)
    {
        _restart = restart;

        if (state.DoorOpen)
        {
            Title   = "ACCÈS ACCORDÉ";
            Message = state.Rank == GameState.PlayerRank.Officier
                ? $"Le garde vous a laissé passer avec déférence, {state.PlayerName}."
                : $"Vous avez convaincu le garde, {state.PlayerName}. La porte s'ouvre.";
        }
        else if (state.Alarme)
        {
            Title   = "ALARME DÉCLENCHÉE";
            Message = $"Vous avez été reconnu, {state.PlayerName}. Le garde appelle du renfort.";
        }
        else if (state.PlayerFled)
        {
            Title   = "FUITE";
            Message = $"{state.PlayerName} prend ses jambes à son cou. Peut-être une meilleure idée.";
        }
        else if (state.MessageDelivered)
        {
            Title   = "MESSAGE REMIS";
            Message = $"Le garde a accepté de transmettre votre message, {state.PlayerName}.";
        }
        else
        {
            Title   = "ACCÈS REFUSÉ";
            Message = $"Vous n'avez pas pu passer, {state.PlayerName}.";
        }
    }

    public void Restart() => _restart();
}
