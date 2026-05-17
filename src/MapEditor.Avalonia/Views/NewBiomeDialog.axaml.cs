using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using MapEditor.Avalonia.ViewModels;

namespace MapEditor.Avalonia.Views;

public partial class NewBiomeDialog : Window
{
    public NewBiomeDialog() => AvaloniaXamlLoader.Load(this);

    private void OnCreate(object? sender, RoutedEventArgs e)
    {
        var id = this.FindControl<TextBox>("IdBox")?.Text?.Trim() ?? "";
        var name = this.FindControl<TextBox>("NameBox")?.Text?.Trim() ?? "";

        if (string.IsNullOrEmpty(id)) return;

        Close(new NewBiomeDialogResult(id, name.Length > 0 ? name : id));
    }

    private void OnCancel(object? sender, RoutedEventArgs e)
        => Close(null);
}