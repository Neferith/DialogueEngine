using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Sample1.ViewModels;

namespace Sample1.Views;

public partial class OutcomeView : UserControl
{
    public OutcomeView() => AvaloniaXamlLoader.Load(this);

    private void OnRestartClick(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        if (DataContext is OutcomeViewModel vm)
            vm.Restart();
    }
}
