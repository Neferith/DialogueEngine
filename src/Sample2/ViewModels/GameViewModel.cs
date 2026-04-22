using CommunityToolkit.Mvvm.ComponentModel;

namespace Sample2.ViewModels;

public sealed partial class GameViewModel : ObservableObject
{
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsSetup), nameof(IsMap), nameof(IsOutcome))]
    private object? _currentPhase;

    public bool IsSetup   => CurrentPhase is SetupViewModel;
    public bool IsMap     => CurrentPhase is MapViewModel;
    public bool IsOutcome => CurrentPhase is OutcomeViewModel;

    public GameViewModel()
    {
        var setup = new SetupViewModel();
        setup.GameStarted += StartMap;
        CurrentPhase = setup;
    }

    private void StartMap(GameState.GameState state)
    {
        var map = new MapViewModel(state);
        map.GameCompleted += () => CurrentPhase = new OutcomeViewModel(state, Restart);
        CurrentPhase = map;
    }

    private void Restart()
    {
        var setup = new SetupViewModel();
        setup.GameStarted += StartMap;
        CurrentPhase = setup;
    }
}

// ─────────────────────────────────────────────────────────────────────────────

public sealed class OutcomeViewModel
{
    private readonly Action _restart;

    public string Title   { get; }
    public string Message { get; }
    public string EmotionLabel { get; }

    public OutcomeViewModel(GameState.GameState state, Action restart)
    {
        _restart = restart;

        Title = "ACCÈS ACCORDÉ";

        EmotionLabel = state.OfficerEmotion switch
        {
            GameState.OfficerEmotion.Charmed   => "CHARME",
            GameState.OfficerEmotion.Scared    => "INTIMIDATION",
            GameState.OfficerEmotion.Convinced => "BLUFF",
            _                                  => "???"
        };

        Message = state.OfficerEmotion switch
        {
            GameState.OfficerEmotion.Charmed =>
                $"{state.PlayerName} a charmé le lieutenant.\nIl vous a donné le laissez-passer volontiers,\nle cœur battant un peu plus vite.",
            GameState.OfficerEmotion.Scared =>
                $"{state.PlayerName} a intimidé le lieutenant.\nIl tremblait en signant le laissez-passer.\nIl évite votre regard.",
            GameState.OfficerEmotion.Convinced =>
                $"{state.PlayerName} a bluffé le lieutenant.\nConvaincu d'un contrôle officiel,\nil a obtempéré sans poser de questions.",
            _ =>
                $"{state.PlayerName} a passé la porte."
        };
    }

    public void Restart() => _restart();
}
