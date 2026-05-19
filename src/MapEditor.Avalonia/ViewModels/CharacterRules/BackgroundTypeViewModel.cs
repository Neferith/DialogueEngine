using Avalonia;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MapEditor.Core.Characters;
using System.Collections.ObjectModel;
using System.Xml.Linq;

namespace MapEditor.Avalonia.ViewModels.CharacterRules;

public partial class BackgroundTypeViewModel : ObservableObject
{
    [ObservableProperty] private string _id = "";
    [ObservableProperty] private string _name = "";
    [ObservableProperty] private string _description = "";

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(RemoveBackgroundCommand))]
    private BackgroundEditorViewModel? _selectedBackground;

    public ObservableCollection<BackgroundEditorViewModel> Backgrounds { get; } = new();

    public BackgroundTypeViewModel() { }

    public BackgroundTypeViewModel(BackgroundTypeData data)
    {
        _id = data.Id;
        _name = data.Name;
        _description = data.Description;

        foreach (var b in data.Backgrounds)
            Backgrounds.Add(new BackgroundEditorViewModel(b));
    }

    [RelayCommand]
    private void AddBackground()
    {
        var vm = new BackgroundEditorViewModel { TypeId = Id };
        Backgrounds.Add(vm);
        SelectedBackground = vm;
    }

    [RelayCommand(CanExecute = nameof(CanRemoveBackground))]
    private void RemoveBackground()
    {
        if (SelectedBackground == null) return;
        Backgrounds.Remove(SelectedBackground);
        SelectedBackground = Backgrounds.LastOrDefault();
    }

    private bool CanRemoveBackground() => SelectedBackground != null;

    public BackgroundTypeData ToData() => new()
    {
        Id = Id.Trim(),
        Name = Name.Trim(),
        Description = Description.Trim(),
        Backgrounds = Backgrounds.Select(b => b.ToData()).ToList()
    };
}