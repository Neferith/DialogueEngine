using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using MapEditor.Avalonia.ViewModels;

namespace MapEditor.Avalonia.Views;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        AvaloniaXamlLoader.Load(this);

        var mapList = this.FindControl<ListBox>("MapList");
        if (mapList != null)
            mapList.DoubleTapped += OnMapListDoubleTapped;
    }

    private void OnMapListDoubleTapped(object? sender, RoutedEventArgs e)
    {
        if (DataContext is not EditorViewModel vm) return;
        if (vm.MapBrowser?.SelectedMap is { } summary)
            vm.MapBrowser.OpenMapCommand.Execute(summary);
    }
}