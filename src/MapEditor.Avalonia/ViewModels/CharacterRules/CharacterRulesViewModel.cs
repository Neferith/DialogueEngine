using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MapEditor.Core.Characters;

namespace MapEditor.Avalonia.ViewModels.CharacterRules;

public partial class CharacterRulesViewModel : ObservableObject
{
    private readonly string _filePath;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(RemoveBackgroundTypeCommand))]
    private BackgroundTypeViewModel? _selectedBackgroundType;

    public ObservableCollection<BackgroundTypeViewModel> BackgroundTypes { get; } = new();
    public SkillsViewModel Skills { get; private set; } = new();

    public CharacterRulesViewModel(string filePath)
    {
        _filePath = filePath;
        Load();
    }

    // ── Chargement ────────────────────────────────────────────────────────────

    private void Load()
    {
        var data = CharacterRulesSerializer.Load(_filePath);

        // Créer le fichier s'il n'existe pas
        if (!File.Exists(_filePath))
            CharacterRulesSerializer.Save(_filePath, data);

        BackgroundTypes.Clear();
        foreach (var t in data.BackgroundTypes)
            BackgroundTypes.Add(new BackgroundTypeViewModel(t));

        Skills = new SkillsViewModel(data.Skills);
        OnPropertyChanged(nameof(Skills));

        SelectedBackgroundType = BackgroundTypes.FirstOrDefault();
    }

    // ── BackgroundTypes ───────────────────────────────────────────────────────

    [RelayCommand]
    private void AddBackgroundType()
    {
        var vm = new BackgroundTypeViewModel { Id = "new_type", Name = "Nouveau type" };
        BackgroundTypes.Add(vm);
        SelectedBackgroundType = vm;
    }

    [RelayCommand(CanExecute = nameof(CanRemoveBackgroundType))]
    private void RemoveBackgroundType()
    {
        if (SelectedBackgroundType == null) return;
        BackgroundTypes.Remove(SelectedBackgroundType);
        SelectedBackgroundType = BackgroundTypes.LastOrDefault();
    }

    private bool CanRemoveBackgroundType() => SelectedBackgroundType != null;

    // ── Sauvegarde ────────────────────────────────────────────────────────────

    [RelayCommand]
    private void Save()
    {
        var data = new CharacterRulesFile
        {
            BackgroundTypes = BackgroundTypes.Select(t => t.ToData()).ToList(),
            Skills = Skills.ToData()
        };
        CharacterRulesSerializer.Save(_filePath, data);
    }
}