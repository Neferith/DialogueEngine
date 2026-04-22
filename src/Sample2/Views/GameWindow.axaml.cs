using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Markup.Xaml;
using Sample2.ViewModels;

namespace Sample2.Views;

public partial class GameWindow : Window
{
    public GameWindow() => AvaloniaXamlLoader.Load(this);

    protected override void OnKeyDown(KeyEventArgs e)
    {
        base.OnKeyDown(e);

        if (DataContext is not GameViewModel gvm) return;

        // Phase dialogue (overlay)
        if (gvm.CurrentPhase is MapViewModel map && map.IsDialogueActive)
        {
            switch (e.Key)
            {
                case Key.Up:                       map.ActiveDialogue?.MoveSelection(-1); break;
                case Key.Down:                     map.ActiveDialogue?.MoveSelection(+1); break;
                case Key.Return or Key.Space or Key.E: map.ActiveDialogue?.ConfirmSelection(); break;
            }
            e.Handled = true;
            return;
        }

        // Phase map — interaction
        if (gvm.CurrentPhase is MapViewModel map2)
        {
            if (e.Key is Key.E or Key.Return)
                map2.TryInteract();
        }
    }
}
