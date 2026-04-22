using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DialogueEngine.Core.Models;

namespace DialogueEngine.Editor.ViewModels;

public interface INodeListFactory
{
    NodeListViewModel Create(DialogueFile file);
}

public sealed partial class NodeListViewModel : ObservableObject
{
    private readonly INodeEditorFactory _factory;

    [ObservableProperty] private string _dialogueId = string.Empty;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HasSelection))]
    private NodeEditorViewModel? _selectedNode;

    public bool HasSelection => SelectedNode is not null;

    public ObservableCollection<NodeEditorViewModel> Nodes { get; } = [];

    public NodeListViewModel(DialogueFile file, INodeEditorFactory factory)
    {
        _factory    = factory;
        _dialogueId = file.Id;

        foreach (var n in file.Nodes)
            Nodes.Add(factory.CreateEditor(n));
    }

    [RelayCommand]
    private void SelectNode(NodeEditorViewModel vm) => SelectedNode = vm;

    [RelayCommand]
    private void AddNode()
    {
        var node = new Node
        {
            Id   = $"node_{Nodes.Count + 1:D2}",
            Text = LocalizedText.Simple(string.Empty)
        };
        var vm = _factory.CreateEditor(node);
        Nodes.Add(vm);
        SelectedNode = vm;
    }

    [RelayCommand]
    private void DeleteNode(NodeEditorViewModel vm)
    {
        Nodes.Remove(vm);
        if (SelectedNode == vm)
            SelectedNode = Nodes.LastOrDefault();
    }

    [RelayCommand]
    private void DuplicateNode(NodeEditorViewModel vm)
    {
        var copy = _factory.CreateEditor(vm.ToModel() with { Id = vm.Id + "_copy" });
        Nodes.Add(copy);
        SelectedNode = copy;
    }

    [RelayCommand]
    private void MoveNodeUp(NodeEditorViewModel vm)   => MoveNode(vm, -1);

    [RelayCommand]
    private void MoveNodeDown(NodeEditorViewModel vm) => MoveNode(vm, +1);

    private void MoveNode(NodeEditorViewModel vm, int delta)
    {
        var i = Nodes.IndexOf(vm);
        var j = i + delta;
        if (j >= 0 && j < Nodes.Count) Nodes.Move(i, j);
    }

    public DialogueFile BuildFile() => new()
    {
        Id    = DialogueId,
        Nodes = Nodes.Select(n => n.ToModel()).ToArray()
    };
}
