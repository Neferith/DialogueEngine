using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Markup.Xaml;
using Sample1.ViewModels;

namespace Sample1.Views;

public partial class GameWindow : Window
{
    public GameWindow() => AvaloniaXamlLoader.Load(this);

    protected override void OnKeyDown(KeyEventArgs e)
    {
        base.OnKeyDown(e);

        if (DataContext is not GameViewModel gvm) return;
        if (gvm.CurrentPhase is not DialogueViewModel dvm) return;

        switch (e.Key)
        {
            case Key.Up:   dvm.MoveSelection(-1); break;
            case Key.Down: dvm.MoveSelection(+1); break;
            case Key.Enter or Key.Space: dvm.ConfirmSelection(); break;
        }
    }
}
