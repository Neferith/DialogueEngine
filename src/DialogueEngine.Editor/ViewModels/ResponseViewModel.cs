using CommunityToolkit.Mvvm.ComponentModel;
using DialogueEngine.Core.Models;

namespace DialogueEngine.Editor.ViewModels;

public sealed partial class ResponseViewModel : ObservableObject
{
    [ObservableProperty] private string _text          = string.Empty;
    [ObservableProperty] private string _conditionKey  = string.Empty;
    [ObservableProperty] private string _consequenceKey = string.Empty;

    /// <summary>
    /// IDs des nœuds suivants, un par ligne.
    /// Le moteur prend le premier dont la condition passe.
    /// </summary>
    [ObservableProperty] private string _nextNodeIds = string.Empty;

    public ResponseViewModel() { }

    public ResponseViewModel(Response model)
    {
        _text           = model.Text.SimpleText ?? string.Empty;
        _conditionKey   = model.ConditionKey    ?? string.Empty;
        _consequenceKey = model.ConsequenceKey  ?? string.Empty;
        _nextNodeIds    = string.Join(Environment.NewLine, model.NextNodeIds);
    }

    public Response ToModel() => new()
    {
        Text           = LocalizedText.Simple(Text),
        ConditionKey   = NullIfEmpty(ConditionKey),
        ConsequenceKey = NullIfEmpty(ConsequenceKey),
        NextNodeIds    = NextNodeIds
                            .Split(Environment.NewLine, StringSplitOptions.RemoveEmptyEntries)
                            .Select(s => s.Trim())
                            .Where(s => s.Length > 0)
                            .ToArray()
    };

    private static string? NullIfEmpty(string s)
        => string.IsNullOrWhiteSpace(s) ? null : s;
}
