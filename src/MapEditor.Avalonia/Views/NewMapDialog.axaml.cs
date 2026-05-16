using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using MapEditor.Avalonia.ViewModels;

namespace MapEditor.Avalonia.Views;

public partial class NewMapDialog : Window
{
    public NewMapDialog() => AvaloniaXamlLoader.Load(this);

    private void OnCreate(object? sender, RoutedEventArgs e)
    {
        if (DataContext is NewMapDialogViewModel vm)
            Close(vm.BuildResult());
    }

    private void OnCancel(object? sender, RoutedEventArgs e)
        => Close(null);
}
