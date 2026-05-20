using Avalonia.Controls;
using MapEditor.Avalonia.ViewModels.Events;
using Avalonia.Interactivity;

namespace MapEditor.Avalonia.Views.Events;

public partial class EventsWindow : Window
{
    public EventsWindow() => InitializeComponent();

    private void OnFileSelected(object? sender, SelectionChangedEventArgs e)
    {
        if (DataContext is EventsViewModel vm &&
            e.AddedItems.Count > 0 &&
            e.AddedItems[0] is EventFileViewModel fileVm)
            vm.SelectFile(fileVm);
    }

    private void OnNewMapFileClick(object? sender, RoutedEventArgs e)
    {
        var box = this.FindControl<TextBox>("NewMapIdBox");
        if (box?.Text?.Trim().Length > 0 && DataContext is EventsViewModel vm)
        {
            vm.NewMapFileCommand.Execute(box.Text.Trim());
            box.Text = "";
        }
    }
}