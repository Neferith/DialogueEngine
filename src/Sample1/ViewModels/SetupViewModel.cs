using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Sample1.GameState;

namespace Sample1.ViewModels;

public sealed partial class SetupViewModel : ObservableObject
{
    [ObservableProperty] private string     _playerName    = "Joueur";
    [ObservableProperty] private PlayerRank _selectedRank  = PlayerRank.Civil;
    [ObservableProperty] private int        _rankCursor    = 0;

    public static PlayerRank[] Ranks { get; } =
        [PlayerRank.Civil, PlayerRank.Soldat, PlayerRank.Officier, PlayerRank.Déserteur];

    public bool IsCivil     => SelectedRank == PlayerRank.Civil;
    public bool IsSoldat    => SelectedRank == PlayerRank.Soldat;
    public bool IsOfficier  => SelectedRank == PlayerRank.Officier;
    public bool IsDéserteur => SelectedRank == PlayerRank.Déserteur;

    public event Action<GameState.GameState>? GameStarted;

    [RelayCommand]
    private void SelectRank(string rank)
    {
        if (Enum.TryParse<PlayerRank>(rank, out var r))
        {
            SelectedRank = r;
            OnPropertyChanged(nameof(IsCivil));
            OnPropertyChanged(nameof(IsSoldat));
            OnPropertyChanged(nameof(IsOfficier));
            OnPropertyChanged(nameof(IsDéserteur));
        }
    }

    [RelayCommand]
    private void Start()
    {
        if (string.IsNullOrWhiteSpace(PlayerName)) PlayerName = "Joueur";

        var state = new GameState.GameState
        {
            PlayerName = PlayerName.Trim(),
            Rank       = SelectedRank,
            HasPass    = SelectedRank == PlayerRank.Officier // les officiers ont toujours un laissez-passer
        };

        GameStarted?.Invoke(state);
    }
}
