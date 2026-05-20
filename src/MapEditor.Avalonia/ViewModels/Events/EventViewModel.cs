using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MapEditor.Core.Events;

namespace MapEditor.Avalonia.ViewModels.Events;

public partial class EventViewModel : ObservableObject
{
    [ObservableProperty] private string _id = "";
    [ObservableProperty] private string _trigger = "MapEnter";
    [ObservableProperty] private string _mapId = "";
    [ObservableProperty] private string _entityId = "";
    [ObservableProperty] private string _tileX = "";
    [ObservableProperty] private string _tileY = "";
    [ObservableProperty] private string _radius = "";

    // Condition
    [ObservableProperty] private string _flagNotSet = "";
    [ObservableProperty] private string _flagSet = "";
    [ObservableProperty] private string _npcAlive = "";
    [ObservableProperty] private string _npcNotAlive = "";
    [ObservableProperty] private string _minTurn = "";

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(RemoveEffectCommand))]
    private EventEffectViewModel? _selectedEffect;

    public ObservableCollection<EventEffectViewModel> Effects { get; } = new();

    public static IReadOnlyList<string> AvailableTriggers =>
    [
        "GameStart", "MapEnter", "TileEnter",
        "TurnPassed", "Interact", "Proximity"
    ];

    public EventViewModel() { }

    public EventViewModel(EventData data)
    {
        _id = data.Id;
        _trigger = data.Trigger;
        _mapId = data.MapId ?? "";
        _entityId = data.EntityId ?? "";
        _tileX = data.TileX?.ToString() ?? "";
        _tileY = data.TileY?.ToString() ?? "";
        _radius = data.Radius?.ToString() ?? "";

        _flagNotSet = data.Condition.FlagNotSet ?? "";
        _flagSet = data.Condition.FlagSet ?? "";
        _npcAlive = data.Condition.NpcAlive ?? "";
        _npcNotAlive = data.Condition.NpcNotAlive ?? "";
        _minTurn = data.Condition.MinTurn?.ToString() ?? "";

        foreach (var e in data.Effects)
            Effects.Add(new EventEffectViewModel(e));
    }

    [RelayCommand]
    private void AddEffect()
    {
        var vm = new EventEffectViewModel { ScriptId = "SetFlag" };
        Effects.Add(vm);
        SelectedEffect = vm;
    }

    [RelayCommand(CanExecute = nameof(CanRemoveEffect))]
    private void RemoveEffect()
    {
        if (SelectedEffect == null) return;
        Effects.Remove(SelectedEffect);
        SelectedEffect = Effects.LastOrDefault();
    }

    private bool CanRemoveEffect() => SelectedEffect != null;

    public EventData ToData() => new()
    {
        Id = Id.Trim(),
        Trigger = Trigger,
        MapId = string.IsNullOrWhiteSpace(MapId) ? null : MapId.Trim(),
        EntityId = string.IsNullOrWhiteSpace(EntityId) ? null : EntityId.Trim(),
        TileX = int.TryParse(TileX, out var x) ? x : null,
        TileY = int.TryParse(TileY, out var y) ? y : null,
        Radius = int.TryParse(Radius, out var r) ? r : null,
        Condition = new EventConditionData
        {
            FlagNotSet = Nullify(FlagNotSet),
            FlagSet = Nullify(FlagSet),
            NpcAlive = Nullify(NpcAlive),
            NpcNotAlive = Nullify(NpcNotAlive),
            MinTurn = int.TryParse(MinTurn, out var t) ? t : null
        },
        Effects = Effects.Select(e => e.ToData()).ToList()
    };

    private static string? Nullify(string s) =>
        string.IsNullOrWhiteSpace(s) ? null : s.Trim();
}