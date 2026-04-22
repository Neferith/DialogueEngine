using Avalonia.Controls;
using DialogueEngine.Core.Models;
using DialogueEngine.Editor.ViewModels;

namespace DialogueEngine.Editor;

/// <summary>
/// Container unique. Implémente INodeListFactory et INodeEditorFactory.
/// Passe "this" partout — chaque VM ne voit que l'interface dont il a besoin.
/// </summary>
public sealed class EditorContainer : INodeListFactory, INodeEditorFactory
{
    private readonly Window _window;

    public EditorContainer(Window window) => _window = window;

    // ── INodeListFactory ──────────────────────────────────────────────────

    public NodeListViewModel Create(DialogueFile file)
        => new(file, this);

    // ── INodeEditorFactory ────────────────────────────────────────────────

    public NodeEditorViewModel CreateEditor(Node node)
        => new(node, this);

    public ResponseViewModel CreateResponse(Response response)
        => new(response);

    // ── MainViewModel (câblé dans App.axaml.cs) ───────────────────────────

    public MainViewModel CreateMain()
        => new(_window, this);
}
