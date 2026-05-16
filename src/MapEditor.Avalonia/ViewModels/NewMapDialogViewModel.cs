using CommunityToolkit.Mvvm.ComponentModel;
using MapEditor.Core.Modules;
using System.Collections.ObjectModel;

namespace MapEditor.Avalonia.ViewModels;

public record NewMapDialogResult(
    string Id,
    int    Width,
    int    Height,
    string ModuleId,
    string DefaultTileTypeId);

public partial class NewMapDialogViewModel : ObservableObject
{
    [ObservableProperty] private string  _mapId   = "new_map";
    [ObservableProperty] private decimal _width   = 16;
    [ObservableProperty] private decimal _height  = 16;
    [ObservableProperty] private ModuleDefinition?      _selectedModule;
    [ObservableProperty] private TileTypeDefinition?    _selectedDefaultTile;

    public ObservableCollection<ModuleDefinition>   Modules   { get; } = new();
    public ObservableCollection<TileTypeDefinition> TileTypes { get; } = new();

    public NewMapDialogViewModel(IReadOnlyList<ModuleDefinition> modules)
    {
        foreach (var m in modules) Modules.Add(m);
        SelectedModule = Modules.FirstOrDefault();
    }

    partial void OnSelectedModuleChanged(ModuleDefinition? value)
    {
        TileTypes.Clear();
        if (value == null) return;
        foreach (var tt in value.TileTypes) TileTypes.Add(tt);
        SelectedDefaultTile = TileTypes.FirstOrDefault();
    }

    public bool IsValid =>
        !string.IsNullOrWhiteSpace(MapId)
        && Width  > 0
        && Height > 0
        && SelectedModule      != null
        && SelectedDefaultTile != null;

    public NewMapDialogResult? BuildResult() =>
        IsValid
            ? new NewMapDialogResult(MapId.Trim(), (int)Width, (int)Height,
                                     SelectedModule!.Id, SelectedDefaultTile!.Id)
            : null;
}
