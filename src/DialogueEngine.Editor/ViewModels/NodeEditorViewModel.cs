using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DialogueEngine.Core.Models;

namespace DialogueEngine.Editor.ViewModels;

public interface INodeEditorFactory
{
    NodeEditorViewModel CreateEditor(Node node);
    ResponseViewModel   CreateResponse(Response response);
}

public sealed partial class NodeEditorViewModel : ObservableObject
{
    private readonly INodeEditorFactory _factory;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(TreeLabel))]
    private string _id                   = string.Empty;

    [ObservableProperty] private string _conditionKey         = string.Empty;
    [ObservableProperty] private string _consequenceKey       = string.Empty;
    [ObservableProperty] private string _cancelConsequenceKey = string.Empty;
    [ObservableProperty] private string _text                 = string.Empty;

    public string TreeLabel => string.IsNullOrWhiteSpace(Id) ? "(sans id)" : Id;

    public ObservableCollection<ResponseViewModel> Responses { get; } = [];

    public NodeEditorViewModel(Node model, INodeEditorFactory factory)
    {
        _factory              = factory;
        _id                   = model.Id;
        _conditionKey         = model.ConditionKey         ?? string.Empty;
        _consequenceKey       = model.ConsequenceKey       ?? string.Empty;
        _cancelConsequenceKey = model.CancelConsequenceKey ?? string.Empty;
        _text                 = model.Text.SimpleText      ?? string.Empty;

        foreach (var r in model.Responses)
            Responses.Add(factory.CreateResponse(r));
    }

    [RelayCommand]
    private void AddResponse()
    {
        Responses.Add(_factory.CreateResponse(new Response
        {
            Text = LocalizedText.Simple(string.Empty)
        }));
    }

    [RelayCommand]
    private void RemoveResponse(ResponseViewModel vm) => Responses.Remove(vm);

    [RelayCommand]
    private void MoveResponseUp(ResponseViewModel vm)   => Move(vm, -1);

    [RelayCommand]
    private void MoveResponseDown(ResponseViewModel vm) => Move(vm, +1);

    private void Move(ResponseViewModel vm, int delta)
    {
        var i = Responses.IndexOf(vm);
        var j = i + delta;
        if (j >= 0 && j < Responses.Count) Responses.Move(i, j);
    }

    public Node ToModel() => new()
    {
        Id                   = Id,
        ConditionKey         = NullIfEmpty(ConditionKey),
        ConsequenceKey       = NullIfEmpty(ConsequenceKey),
        CancelConsequenceKey = NullIfEmpty(CancelConsequenceKey),
        Text                 = LocalizedText.Simple(Text),
        Responses            = Responses.Select(r => r.ToModel()).ToArray()
    };

    private static string? NullIfEmpty(string s)
        => string.IsNullOrWhiteSpace(s) ? null : s;
}
