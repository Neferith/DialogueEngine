using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Markup.Xaml;
using Avalonia.Threading;
using Sample2.ViewModels;

namespace Sample2.Views;

public partial class MapView : UserControl
{
    private DispatcherTimer? _loop;
    private readonly HashSet<Key> _keys = [];

    public MapView()
    {
        AvaloniaXamlLoader.Load(this);
    }

    protected override void OnAttachedToVisualTree(Avalonia.VisualTreeAttachmentEventArgs e)
    {
        base.OnAttachedToVisualTree(e);

        if (TopLevel.GetTopLevel(this) is Window win)
        {
            win.KeyDown += OnWinKeyDown;
            win.KeyUp   += OnWinKeyUp;
        }

        _loop = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(33) };
        _loop.Tick += OnTick;
        _loop.Start();
    }

    protected override void OnDetachedFromVisualTree(Avalonia.VisualTreeAttachmentEventArgs e)
    {
        base.OnDetachedFromVisualTree(e);

        if (TopLevel.GetTopLevel(this) is Window win)
        {
            win.KeyDown -= OnWinKeyDown;
            win.KeyUp   -= OnWinKeyUp;
        }

        _loop?.Stop();
        _loop = null;
    }

    private void OnWinKeyDown(object? s, KeyEventArgs e) => _keys.Add(e.Key);
    private void OnWinKeyUp(object? s, KeyEventArgs e)   => _keys.Remove(e.Key);

    private void OnTick(object? s, EventArgs e)
    {
        if (DataContext is not MapViewModel vm) return;

        bool left  = _keys.Contains(Key.Left)  || _keys.Contains(Key.Q);
        bool right = _keys.Contains(Key.Right) || _keys.Contains(Key.D);
        bool up    = _keys.Contains(Key.Up)    || _keys.Contains(Key.Z);
        bool down  = _keys.Contains(Key.Down)  || _keys.Contains(Key.S);

        vm.Update(left, right, up, down);
    }
}
