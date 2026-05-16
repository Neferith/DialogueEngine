using MapEditor.Avalonia.ViewModels;
using MapEditor.Core.Modules;
using MapEditor.Core.Serialization;

namespace MapEditor.Avalonia.DI;

public interface IMapSerializerFactory { MapSerializer CreateMapSerializer(); }
public interface IModuleLoaderFactory  { ModuleLoader  CreateModuleLoader();  }

/// <summary>
/// Lightweight DI container — same factory-container pattern as DialogueEngine2.
/// Passes <c>this</c> as the specific interface each class needs.
/// </summary>
public class EditorContainer : IMapSerializerFactory, IModuleLoaderFactory
{
    public MapSerializer CreateMapSerializer() => new();
    public ModuleLoader  CreateModuleLoader()  => new();

    public EditorViewModel CreateEditorViewModel(IDialogService dialogService)
        => new(this, this, dialogService);
}
