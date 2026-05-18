using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MapEditor.Core.Models;
using MapEditor.Core.Serialization;
using System.Collections.ObjectModel;

namespace MapEditor.Avalonia.ViewModels;

public partial class MapBrowserViewModel : ObservableObject
{
    private readonly MapSerializer _serializer;
    private readonly Action<MapSummary> _onOpen;
    private readonly Action<MapSummary> _onSelect;

    [ObservableProperty] private MapSummary? _selectedMap;

    public ObservableCollection<MapSummary> Maps { get; } = new();
    public bool IsEmpty => Maps.Count == 0;

    public MapBrowserViewModel(
        MapSerializer serializer,
        Action<MapSummary> onOpen,    // double-clic → ouvrir dans le canvas
        Action<MapSummary> onSelect)  // simple clic → afficher propriétés
    {
        _serializer = serializer;
        _onOpen = onOpen;
        _onSelect = onSelect;

        Maps.CollectionChanged += (_, _) => OnPropertyChanged(nameof(IsEmpty));
    }

    // ── Chargement ────────────────────────────────────────────────────────────

    public void LoadFromProject(string mapsPath)
    {
        Maps.Clear();
        if (!Directory.Exists(mapsPath)) return;

        foreach (var file in Directory.GetFiles(mapsPath, "*.map.json")
                                       .OrderBy(f => f))
        {
            var mapFile = _serializer.Load(file);
            if (mapFile == null) continue;
            Maps.Add(MapSummary.From(mapFile, file));
        }
    }

    public void Refresh(string mapsPath) => LoadFromProject(mapsPath);

    // ── Commandes ─────────────────────────────────────────────────────────────

    [RelayCommand]
    private void SelectMap(MapSummary? map)
    {
        if (map == null) return;
        SelectedMap = map;
        _onSelect(map);
    }

    [RelayCommand]
    private void OpenMap(MapSummary? map)
    {
        if (map == null) return;
        SelectedMap = map;
        _onOpen(map);
    }
}