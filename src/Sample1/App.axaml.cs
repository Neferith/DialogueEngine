using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using Sample1.ViewModels;
using Sample1.Views;

namespace Sample1;

public sealed class App : Application
{
    public override void Initialize() => AvaloniaXamlLoader.Load(this);

    public override void OnFrameworkInitializationCompleted()
    {
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            desktop.MainWindow = new GameWindow
            {
                DataContext = new GameViewModel()
            };
        }
        base.OnFrameworkInitializationCompleted();
    }
}
