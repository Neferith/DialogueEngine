using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MapEditor.Avalonia;
using MapEditor.Avalonia.DI;
using MapEditor.Avalonia.ViewModels.CharacterRules;
using MapEditor.Core.Models;
using MapEditor.Core.Modules;
using MapEditor.Core.Serialization;
using System.Collections.ObjectModel;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace MapEditor.Avalonia.ViewModels;

public enum EditorTool { PaintTile, PaintEntity, Erase, Select }

public partial class EditorViewModel : ObservableObject
{
    private readonly MapSerializer _serializer;
    private readonly ModuleLoader  _loader;
    private readonly IDialogService _dialog;

    private readonly RecentProjectsService _recentProjects = new();

    public IReadOnlyList<RecentProject> RecentProjects => _recentProjects.Projects;

    private string? _currentFilePath;

    [ObservableProperty] private MapGridViewModel?   _mapGrid;
    [ObservableProperty] private PropertiesViewModel _properties = new();
    [ObservableProperty] private EditorTool          _activeTool = EditorTool.PaintTile;
    [ObservableProperty] private ModuleDefinition?   _activeModule;
    [ObservableProperty] private TileTypeDefinition?   _selectedTileType;
    [ObservableProperty] private EntityTypeDefinition? _selectedEntityType;
    [ObservableProperty] private string _statusText = "Crée ou ouvre une map pour commencer.";
    [ObservableProperty] private CampaignProject? _activeProject;
    [ObservableProperty] private string _projectName = "Aucun projet";
    [ObservableProperty] private MapBrowserViewModel? _mapBrowser;
    [ObservableProperty] private CharacterRulesViewModel? _characterRules;

    public ObservableCollection<ModuleDefinition>   Modules     { get; } = new();
    public ObservableCollection<TileTypeDefinition>   TileTypes   { get; } = new();
    public ObservableCollection<EntityTypeDefinition> EntityTypes { get; } = new();

    // Tool toggle bindings for the toolbar
    public bool IsPaintTileActive   => ActiveTool == EditorTool.PaintTile;
    public bool IsPaintEntityActive => ActiveTool == EditorTool.PaintEntity;
    public bool IsEraseActive       => ActiveTool == EditorTool.Erase;
    public bool IsSelectActive      => ActiveTool == EditorTool.Select;

    public event Action? EventsOpenRequested;

    public event Action? ItemsOpenRequested;



    public EditorViewModel(IMapSerializerFactory serFactory,
                           IModuleLoaderFactory  loaderFactory,
                           IDialogService        dialog)
    {
        _serializer = serFactory.CreateMapSerializer();
        _loader     = loaderFactory.CreateModuleLoader();
        _dialog     = dialog;

        MapBrowser = new MapBrowserViewModel(
          _serializer,
          onOpen: summary => OpenMapFromSummary(summary),
          onSelect: summary => Properties.ShowMapInfo(summary));
    }

    // ── Module loading ────────────────────────────────────────────────────────

    private void LoadModulesFromProject(CampaignProject project)
    {
        Modules.Clear();
        var loaded = _loader.LoadAll(project.AbsoluteModulesPath);
        foreach (var m in loaded) Modules.Add(m);
        ActiveModule = Modules.FirstOrDefault();
        StatusText = loaded.Count == 0
            ? $"Aucun module trouvé dans {project.AbsoluteModulesPath}"
            : $"Projet «{project.Name}» — {loaded.Count} biome(s) chargé(s).";
    }

    private void LoadModules()
    {
        var modulesPath = Path.Combine(AppContext.BaseDirectory, "modules");
        var loaded      = _loader.LoadAll(modulesPath);
        foreach (var m in loaded) Modules.Add(m);

        ActiveModule = Modules.FirstOrDefault();
        StatusText   = loaded.Count == 0
            ? $"Aucun module trouvé dans {modulesPath}"
            : $"{loaded.Count} module(s) chargé(s).";
    }

    partial void OnActiveModuleChanged(ModuleDefinition? value)
    {
        TileTypes.Clear();
        EntityTypes.Clear();
        if (value == null) return;
        foreach (var t in value.TileTypes)   TileTypes.Add(t);
        foreach (var e in value.EntityTypes) EntityTypes.Add(e);
        SelectedTileType   = TileTypes.FirstOrDefault();
        SelectedEntityType = EntityTypes.FirstOrDefault();
    }

    partial void OnActiveToolChanged(EditorTool value)
    {
        OnPropertyChanged(nameof(IsPaintTileActive));
        OnPropertyChanged(nameof(IsPaintEntityActive));
        OnPropertyChanged(nameof(IsEraseActive));
        OnPropertyChanged(nameof(IsSelectActive));
    }

    // ── Toolbar commands ──────────────────────────────────────────────────────

    [RelayCommand] void SetTool(string tool) =>
        ActiveTool = Enum.TryParse<EditorTool>(tool, out var t) ? t : EditorTool.PaintTile;

    // ── File commands ─────────────────────────────────────────────────────────

    [RelayCommand]
    private async Task OpenProject()
    {
        var path = await _dialog.ShowOpenProjectDialog();
        if (path == null) return;

        var project = _serializer.LoadProject(path);
        if (project == null) { StatusText = "Erreur : impossible de lire le projet."; return; }

        ActiveProject = project;
        ProjectName = project.Name;
        _recentProjects.Add(project.Name, path);
        OnPropertyChanged(nameof(RecentProjects));
        LoadModulesFromProject(project);

        var mapPaths = ScanMaps(project);
        Properties.Initialize(_serializer, mapPaths);
        MapBrowser?.LoadFromProject(project.AbsoluteMapsPath);
        CharacterRules = new CharacterRulesViewModel(project.AbsoluteCharacterRulesPath);

        Properties.Clear();
        MapGrid = null;
        SaveMapCommand.NotifyCanExecuteChanged();
        SaveMapAsCommand.NotifyCanExecuteChanged();
    }

    public event Action? CharacterRulesOpenRequested;

    [RelayCommand(CanExecute = nameof(HasProject))]
    private void OpenCharacterRules()
    {
        CharacterRulesOpenRequested?.Invoke();
    }

    [RelayCommand(CanExecute = nameof(HasProject))]
    private async Task NewBiome()
    {
        var result = await _dialog.ShowNewBiomeDialog();
        if (result == null || ActiveProject == null) return;

        // Créer le dossier + module.json
        var biomeDir = Path.Combine(ActiveProject.AbsoluteModulesPath, result.Id);
        Directory.CreateDirectory(biomeDir);
        Directory.CreateDirectory(Path.Combine(biomeDir, "textures"));

        var module = new ModuleDefinition
        {
            Id = result.Id,
            Name = result.Name,
            SpriteSize = 16,
            Textures = new ModuleTextures
            {
                Wall = "textures/wall.png",
                Floor = "textures/floor.png",
                Ceiling = "textures/ceiling.png",
                DoorClosed = "textures/door_closed.png",
                DoorOpen = "textures/door_open.png"
            }
        };

        var modulePath = Path.Combine(biomeDir, "module.json");
        File.WriteAllText(modulePath, JsonSerializer.Serialize(module, new JsonSerializerOptions
        {
            WriteIndented = true,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
        }));

        // Recharger les modules
        LoadModulesFromProject(ActiveProject);
        StatusText = $"Biome «{result.Name}» créé.";
    }

    private bool HasProject() => ActiveProject != null;

    [RelayCommand(CanExecute = nameof(HasProject))]
    private async Task NewMap()
    {
        if (Modules.Count == 0) { StatusText = "Aucun module disponible."; return; }

        var result = await _dialog.ShowNewMapDialog(Modules);
        if (result == null || ActiveProject == null) return;

        var module = Modules.First(m => m.Id == result.ModuleId);
        var mapFile = new MapFile
        {
            Id = result.Id,
            ModuleId = result.ModuleId,
            Size = new SizeData(result.Width, result.Height),
            DefaultTileTypeId = result.DefaultTileTypeId
        };

        _currentFilePath = null;
        OpenMap(mapFile, module);
        StatusText = $"Nouvelle map «{result.Id}» ({result.Width}×{result.Height}).";
    }

    [RelayCommand]
    private async Task OpenMap()
    {
        var path = await _dialog.ShowOpenFileDialog();
        if (path == null) return;

        var mapFile = _serializer.Load(path);
        if (mapFile == null) { StatusText = "Erreur : impossible de lire le fichier."; return; }

        var module = Modules.FirstOrDefault(m => m.Id == mapFile.ModuleId);
        if (module == null) { StatusText = $"Module « {mapFile.ModuleId} » introuvable."; return; }

        _currentFilePath = path;
        OpenMap(mapFile, module);
        StatusText = $"Map « {mapFile.Id} » ouverte depuis {Path.GetFileName(path)}.";
    }

    [RelayCommand(CanExecute = nameof(HasOpenMap))]
    private async Task SaveMap()
    {
        if (_currentFilePath == null) { await SaveMapAs(); return; }
        _serializer.Save(MapGrid!.MapFile, _currentFilePath);
        StatusText = $"Sauvegardé : {Path.GetFileName(_currentFilePath)}.";
        if (ActiveProject != null)
        {
            Properties.Initialize(_serializer, ScanMaps(ActiveProject));
            MapBrowser?.Refresh(ActiveProject.AbsoluteMapsPath);
        }
    }

    [RelayCommand(CanExecute = nameof(HasOpenMap))]
    private async Task SaveMapAs()
    {
        var path = await _dialog.ShowSaveFileDialog($"{MapGrid!.MapFile.Id}.map.json");
        if (path == null) return;
        _currentFilePath = path;
        _serializer.Save(MapGrid.MapFile, path);
        StatusText = $"Sauvegardé : {Path.GetFileName(path)}.";
        if (ActiveProject != null)
        {
            Properties.Initialize(_serializer, ScanMaps(ActiveProject));
            MapBrowser?.Refresh(ActiveProject.AbsoluteMapsPath);
        }
    }

    [RelayCommand]
    private async Task OpenRecentProject(RecentProject recent)
    {
        if (!File.Exists(recent.Path))
        {
            _recentProjects.Remove(recent.Path);
            OnPropertyChanged(nameof(RecentProjects));
            StatusText = $"Introuvable : {recent.Path}";
            return;
        }

        var project = _serializer.LoadProject(recent.Path);
        if (project == null) { StatusText = "Erreur : impossible de lire le projet."; return; }

        ActiveProject = project;
        ProjectName = project.Name;
        LoadModulesFromProject(project);

        var mapPaths = ScanMaps(project);
        Properties.Initialize(_serializer, mapPaths);
        MapBrowser?.LoadFromProject(project.AbsoluteMapsPath);
        CharacterRules = new CharacterRulesViewModel(project.AbsoluteCharacterRulesPath);

        _recentProjects.Add(project.Name, recent.Path);
        OnPropertyChanged(nameof(RecentProjects));

        Properties.Clear();
        MapGrid = null;
        SaveMapCommand.NotifyCanExecuteChanged();
        SaveMapAsCommand.NotifyCanExecuteChanged();
        StatusText = $"Projet «{project.Name}» ouvert.";
    }

    private bool HasOpenMap() => MapGrid != null;

    private void OpenMapFromSummary(MapSummary summary)
    {
        var mapFile = _serializer.Load(summary.FilePath);
        if (mapFile == null) { StatusText = $"Erreur : impossible de lire {summary.Id}."; return; }

        var module = Modules.FirstOrDefault(m => m.Id == mapFile.ModuleId);
        if (module == null) { StatusText = $"Module '{mapFile.ModuleId}' introuvable."; return; }

        _currentFilePath = summary.FilePath;
        OpenMap(mapFile, module);
        StatusText = $"Map «{mapFile.Id}» ouverte.";
    }

    [RelayCommand(CanExecute = nameof(HasProject))]
    private void OpenEvents()
    {
        EventsOpenRequested?.Invoke();
    }

    [RelayCommand(CanExecute = nameof(HasProject))]
    private void OpenItems() => ItemsOpenRequested?.Invoke();

    // ── Canvas interaction (called by MapCanvasControl) ───────────────────────

    public void HandleCellInteraction(int x, int y, bool isDrag)
    {
        if (MapGrid == null || ActiveModule == null) return;

        switch (ActiveTool)
        {
            case EditorTool.PaintTile when SelectedTileType != null:
                MapGrid.PaintTile(x, y, SelectedTileType);
                break;

            case EditorTool.PaintEntity when SelectedEntityType != null && !isDrag:
                MapGrid.PlaceEntity(x, y, SelectedEntityType);
                break;

            case EditorTool.Erase:
                MapGrid.EraseAt(x, y);
                break;

            case EditorTool.Select when !isDrag:
                SelectCell(x, y);
                break;
        }

        StatusText = $"({x}, {y})  outil : {ActiveTool}";
    }

    public void HandleHover(int? x, int? y)
    {
        MapGrid?.SetHover(x, y);
        if (x.HasValue && y.HasValue)
            StatusText = $"({x}, {y})";
    }

    // ── Internals ─────────────────────────────────────────────────────────────

    private void OpenMap(MapFile mapFile, ModuleDefinition module)
    {
        MapGrid     = new MapGridViewModel(mapFile, module);
        ActiveModule = module;
        Properties.Clear();
        SaveMapCommand.NotifyCanExecuteChanged();
        SaveMapAsCommand.NotifyCanExecuteChanged();
    }

    private void SelectCell(int x, int y)
    {
        if (MapGrid == null || ActiveModule == null) return;

        MapGrid.SetSelection(x, y);

        var entity = MapGrid.GetEntityAt(x, y);
        if (entity != null)
        {
            var entityType = ActiveModule.FindEntityType(entity.EntityTypeId);
            if (entityType != null) { Properties.ShowEntity(x, y, entityType, entity, MapGrid); return; }
        }

        var tileType = MapGrid.GetTileTypeAt(x, y) ?? MapGrid.DefaultTileType;
        var tileData = MapGrid.GetTileDataAt(x, y);
        Properties.ShowTile(x, y, tileType, tileData, MapGrid);
    }

    private Dictionary<string, string> ScanMaps(CampaignProject project)
    {
        var result = new Dictionary<string, string>();
        if (!Directory.Exists(project.AbsoluteMapsPath)) return result;

        foreach (var file in Directory.GetFiles(project.AbsoluteMapsPath, "*.map.json"))
        {
            var mapFile = _serializer.Load(file);
            if (mapFile != null)
                result[mapFile.Id] = file;
        }
        return result;
    }
}
