using Avalonia.Controls;
using Avalonia.Platform.Storage;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DialogueEngine.Core.Models;
using DialogueEngine.Serialization;
using System.Collections.ObjectModel;

namespace DialogueEngine.Editor.ViewModels;

public sealed partial class MainViewModel : ObservableObject
{
    private readonly Window          _window;
    private readonly INodeListFactory _listFactory;

    private static readonly FilePickerFileType JsonType = new("Dialogue JSON")
    {
        Patterns  = ["*.json"],
        MimeTypes = ["application/json"]
    };

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HasFile))]
    [NotifyCanExecuteChangedFor(nameof(SaveFileCommand))]
    [NotifyCanExecuteChangedFor(nameof(SaveAsCommand))]
    [NotifyCanExecuteChangedFor(nameof(ValidateCommand))]
    private NodeListViewModel? _nodeList;

    [ObservableProperty] private string _statusMessage = "Prêt.";
    [ObservableProperty] private string? _currentPath;

    public bool HasFile             => NodeList is not null;
    public bool HasValidationErrors => ValidationErrors.Count > 0;

    public ObservableCollection<string> ValidationErrors { get; } = [];

    public MainViewModel(Window window, INodeListFactory listFactory)
    {
        _window      = window;
        _listFactory = listFactory;

        ValidationErrors.CollectionChanged += (_, _)
            => OnPropertyChanged(nameof(HasValidationErrors));
    }

    // ── Nouveau ───────────────────────────────────────────────────────────

    [RelayCommand]
    private void NewFile()
    {
        NodeList      = _listFactory.Create(new DialogueFile { Id = "nouveau_dialogue", Nodes = [] });
        CurrentPath   = null;
        StatusMessage = "Nouveau fichier créé.";
        ValidationErrors.Clear();
    }

    // ── Ouvrir ────────────────────────────────────────────────────────────

    [RelayCommand]
    private async Task OpenFileAsync()
    {
        var picked = await _window.StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions
        {
            Title          = "Ouvrir un fichier de dialogue",
            AllowMultiple  = false,
            FileTypeFilter = [JsonType]
        });

        if (picked.Count == 0) return;

        try
        {
            await using var stream = await picked[0].OpenReadAsync();
            var file = await DialogueFileSerializer.DeserializeAsync(stream);
            NodeList      = _listFactory.Create(file);
            CurrentPath   = picked[0].Path.LocalPath;
            StatusMessage = $"Ouvert : {Path.GetFileName(CurrentPath)}";
            ValidationErrors.Clear();
        }
        catch (Exception ex)
        {
            StatusMessage = $"Erreur : {ex.Message}";
        }
    }

    // ── Sauvegarder ───────────────────────────────────────────────────────

    [RelayCommand(CanExecute = nameof(HasFile))]
    private async Task SaveFileAsync()
    {
        if (NodeList is null) return;

        var path = CurrentPath ?? await PickSavePathAsync(NodeList.DialogueId);
        if (path is null) return;

        await WriteAsync(NodeList.BuildFile(), path);
    }

    [RelayCommand(CanExecute = nameof(HasFile))]
    private async Task SaveAsAsync()
    {
        if (NodeList is null) return;

        var path = await PickSavePathAsync(NodeList.DialogueId);
        if (path is null) return;

        await WriteAsync(NodeList.BuildFile(), path);
    }

    // ── Valider ───────────────────────────────────────────────────────────

    [RelayCommand(CanExecute = nameof(HasFile))]
    private void Validate()
    {
        if (NodeList is null) return;

        var file   = NodeList.BuildFile();
        var errors = new List<string>();

        // Vérifications de base
        if (string.IsNullOrWhiteSpace(file.Id))
            errors.Add("L'ID du dialogue est vide.");

        var ids = file.Nodes.Select(n => n.Id).ToList();

        foreach (var node in file.Nodes)
        {
            if (string.IsNullOrWhiteSpace(node.Id))
                errors.Add("Un nœud a un ID vide.");

            foreach (var response in node.Responses)
            {
                foreach (var nextId in response.NextNodeIds)
                {
                    if (!ids.Contains(nextId))
                        errors.Add($"Nœud '{node.Id}' > réponse : '{nextId}' introuvable.");
                }
            }
        }

        ValidationErrors.Clear();
        foreach (var e in errors) ValidationErrors.Add(e);

        StatusMessage = errors.Count == 0 ? "Validation OK." : $"{errors.Count} erreur(s).";
    }

    // ── Helpers ───────────────────────────────────────────────────────────

    private async Task<string?> PickSavePathAsync(string suggestedName)
    {
        var picked = await _window.StorageProvider.SaveFilePickerAsync(new FilePickerSaveOptions
        {
            Title             = "Enregistrer le dialogue",
            SuggestedFileName = $"{suggestedName}.json",
            FileTypeChoices   = [JsonType]
        });
        return picked?.Path.LocalPath;
    }

    private async Task WriteAsync(DialogueFile file, string path)
    {
        try
        {
            var json = DialogueFileSerializer.Serialize(file);
            await File.WriteAllTextAsync(path, json);
            CurrentPath   = path;
            StatusMessage = $"Sauvegardé : {Path.GetFileName(path)}";
        }
        catch (Exception ex)
        {
            StatusMessage = $"Erreur : {ex.Message}";
        }
    }
}
