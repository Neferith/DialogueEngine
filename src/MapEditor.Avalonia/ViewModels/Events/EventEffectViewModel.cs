using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using MapEditor.Core.Events;

namespace MapEditor.Avalonia.ViewModels.Events;

public partial class EventEffectViewModel : ObservableObject
{
    [ObservableProperty] private string _scriptId = "";
    [ObservableProperty] private string _paramsText = "";

    public EventEffectViewModel() { }

    public EventEffectViewModel(EventEffectData data)
    {
        _scriptId = data.ScriptId;
        _paramsText = string.Join("\n",
            data.Params.Select(kv => $"{kv.Key}={kv.Value}"));
    }

    public EventEffectData ToData() => new()
    {
        ScriptId = ScriptId.Trim(),
        Params = ParseParams(ParamsText)
    };

    private static Dictionary<string, string> ParseParams(string text) =>
        text.Split('\n', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Select(line => line.Split('=', 2))
            .Where(parts => parts.Length == 2)
            .ToDictionary(parts => parts[0].Trim(), parts => parts[1].Trim());
}