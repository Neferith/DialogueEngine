using Avalonia.Controls;
using Avalonia.Platform.Storage;
using MapEditor.Avalonia.ViewModels;
using MapEditor.Avalonia.Views;
using MapEditor.Core.Modules;

namespace MapEditor.Avalonia;

public record NewBiomeDialogResult(string Id, string Name);

public interface IDialogService
{
    Task<NewMapDialogResult?> ShowNewMapDialog(IReadOnlyList<ModuleDefinition> modules);
    Task<string?> ShowOpenFileDialog();
    Task<string?> ShowSaveFileDialog(string suggestedName);
    Task<string?> ShowOpenProjectDialog();
    Task<NewBiomeDialogResult?> ShowNewBiomeDialog();
}

public class AvaloniaDialogService(Window owner) : IDialogService
{
    public async Task<NewMapDialogResult?> ShowNewMapDialog(IReadOnlyList<ModuleDefinition> modules)
    {
        var vm     = new NewMapDialogViewModel(modules);
        var dialog = new NewMapDialog { DataContext = vm };
        return await dialog.ShowDialog<NewMapDialogResult?>(owner);
    }

    public async Task<string?> ShowOpenFileDialog()
    {
        var files = await owner.StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions
        {
            Title             = "Open Map",
            AllowMultiple     = false,
            FileTypeFilter    = [new FilePickerFileType("Map JSON") { Patterns = ["*.map.json"] }]
        });
        return files.Count == 1 ? files[0].Path.LocalPath : null;
    }

    public async Task<string?> ShowSaveFileDialog(string suggestedName)
    {
        var file = await owner.StorageProvider.SaveFilePickerAsync(new FilePickerSaveOptions
        {
            Title           = "Save Map",
            SuggestedFileName = suggestedName,
            FileTypeChoices = [new FilePickerFileType("Map JSON") { Patterns = ["*.map.json"] }]
        });
        return file?.Path.LocalPath;
    }

    public async Task<string?> ShowOpenProjectDialog()
    {
        var files = await owner.StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions
        {
            Title = "Ouvrir un projet campagne",
            AllowMultiple = false,
            FileTypeFilter = [new FilePickerFileType("Campaign") { Patterns = ["*.campaign.json"] }]
        });
        return files.Count == 1 ? files[0].Path.LocalPath : null;
    }

    public async Task<NewBiomeDialogResult?> ShowNewBiomeDialog()
    {
        var dialog = new NewBiomeDialog();
        return await dialog.ShowDialog<NewBiomeDialogResult?>(owner);
    }
}
