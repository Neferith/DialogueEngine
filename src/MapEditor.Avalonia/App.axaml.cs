using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using MapEditor.Avalonia.DI;
using MapEditor.Avalonia.Views;

namespace MapEditor.Avalonia;

public class App : Application
{
    public override void Initialize() => AvaloniaXamlLoader.Load(this);

    public override void OnFrameworkInitializationCompleted()
    {
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            var container  = new EditorContainer();
            var mainWindow = new MainWindow();
            var vm         = container.CreateEditorViewModel(new AvaloniaDialogService(mainWindow));
            mainWindow.DataContext = vm;
            desktop.MainWindow    = mainWindow;
        }
        base.OnFrameworkInitializationCompleted();
    }
}
