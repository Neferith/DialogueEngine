using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MapEditor.Core.Events;

namespace MapEditor.Avalonia.ViewModels.Events;

public partial class EventsViewModel : ObservableObject
{
    private readonly string _eventsPath;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(RemoveEventCommand))]
    private EventViewModel? _selectedEvent;

    [ObservableProperty] private string _currentFile = "";

    public ObservableCollection<EventFileViewModel> Files { get; } = new();
    public ObservableCollection<EventViewModel> Events { get; } = new();

    public EventsViewModel(string eventsPath)
    {
        _eventsPath = eventsPath;
        Load();
    }

    // ── Chargement ────────────────────────────────────────────────────────────

    private void Load()
    {
        Files.Clear();
        Events.Clear();

        var subFolders = new[] { "global", "maps" };
        foreach (var sub in subFolders)
        {
            var dir = Path.Combine(_eventsPath, sub);
            if (!Directory.Exists(dir)) continue;

            foreach (var path in Directory.GetFiles(dir, "*.events.json"))
            {
                var file = EventSerializer.Load(path);
                Files.Add(new EventFileViewModel(path, file));
            }
        }

        if (Files.Count > 0)
            SelectFile(Files[0]);
    }

    // ── Sélection de fichier ──────────────────────────────────────────────────

    public void SelectFile(EventFileViewModel fileVm)
    {
        CurrentFile = fileVm.DisplayName;
        Events.Clear();
        foreach (var ev in fileVm.Events)
            Events.Add(ev);
        SelectedEvent = Events.FirstOrDefault();
    }

    // ── Commandes events ──────────────────────────────────────────────────────

    [RelayCommand]
    private void AddEvent()
    {
        var vm = new EventViewModel { Id = "new_event", Trigger = "MapEnter" };
        Events.Add(vm);
        SelectedEvent = vm;
    }

    [RelayCommand(CanExecute = nameof(CanRemoveEvent))]
    private void RemoveEvent()
    {
        if (SelectedEvent == null) return;
        Events.Remove(SelectedEvent);
        SelectedEvent = Events.LastOrDefault();
    }

    private bool CanRemoveEvent() => SelectedEvent != null;

    // ── Créer un nouveau fichier ──────────────────────────────────────────────

    [RelayCommand]
    private void NewGlobalFile() => CreateFile("global", "game");
    [RelayCommand]
    private void NewMapFile(string mapId) => CreateFile("maps", mapId);

    private void CreateFile(string folder, string name)
    {
        var dir = Path.Combine(_eventsPath, folder);
        Directory.CreateDirectory(dir);
        var path = Path.Combine(dir, $"{name}.events.json");
        EventSerializer.Save(path, new EventFile());
        Load();
    }

    // ── Sauvegarde ────────────────────────────────────────────────────────────

    [RelayCommand]
    private void Save()
    {
        var currentFileVm = Files.FirstOrDefault(f => f.DisplayName == CurrentFile);
        if (currentFileVm == null) return;

        var file = new EventFile
        {
            Events = Events.Select(e => e.ToData()).ToList()
        };
        EventSerializer.Save(currentFileVm.FilePath, file);
    }
}

public class EventFileViewModel
{
    public string FilePath { get; }
    public string DisplayName { get; }
    public List<EventViewModel> Events { get; }

    public EventFileViewModel(string path, EventFile file)
    {
        FilePath = path;
        DisplayName = Path.GetFileNameWithoutExtension(path);
        Events = file.Events.Select(e => new EventViewModel(e)).ToList();
    }
}