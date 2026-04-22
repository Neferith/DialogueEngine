using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Sample2.GameState;

namespace Sample2.ViewModels;

public sealed partial class SetupViewModel : ObservableObject
{
    [ObservableProperty] private string _playerName   = "Joueur";
    [ObservableProperty] private Skill  _selectedSkill = Skill.Charme;

    public bool IsCharme        => SelectedSkill == Skill.Charme;
    public bool IsIntimidation  => SelectedSkill == Skill.Intimidation;
    public bool IsBluff         => SelectedSkill == Skill.Bluff;

    public event Action<GameState.GameState>? GameStarted;

    [RelayCommand]
    private void SelectSkill(string skill)
    {
        if (Enum.TryParse<Skill>(skill, out var s))
        {
            SelectedSkill = s;
            OnPropertyChanged(nameof(IsCharme));
            OnPropertyChanged(nameof(IsIntimidation));
            OnPropertyChanged(nameof(IsBluff));
        }
    }

    [RelayCommand]
    private void Start()
    {
        if (string.IsNullOrWhiteSpace(PlayerName)) PlayerName = "Joueur";
        GameStarted?.Invoke(new GameState.GameState
        {
            PlayerName  = PlayerName.Trim(),
            PlayerSkill = SelectedSkill
        });
    }
}
