using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using DialogueEngine.Editor.Views;

namespace DialogueEngine.Editor;

public sealed class App : Application
{
    public override void Initialize() => AvaloniaXamlLoader.Load(this);

    public override void OnFrameworkInitializationCompleted()
    {
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            var window    = new MainWindow();
            var container = new EditorContainer(window);
            window.DataContext = container.CreateMain();
            desktop.MainWindow = window;
        }

        base.OnFrameworkInitializationCompleted();
    }
}
