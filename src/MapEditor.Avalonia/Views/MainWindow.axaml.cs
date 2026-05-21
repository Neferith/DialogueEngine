using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using MapEditor.Avalonia.ViewModels;
using MapEditor.Avalonia.ViewModels.Events;
using MapEditor.Avalonia.ViewModels.Items;
using MapEditor.Avalonia.Views.Events;
using MapEditor.Avalonia.Views.Items;

namespace MapEditor.Avalonia.Views;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        AvaloniaXamlLoader.Load(this);
        this.Loaded += OnLoaded;

        DataContextChanged += (_, _) =>
        {
            if (DataContext is EditorViewModel vm) { 
                vm.CharacterRulesOpenRequested += () =>
                {
                    var window = new CharacterRulesWindow
                    {
                        DataContext = vm.CharacterRules
                    };
                    window.Show(this);
                };

            vm.EventsOpenRequested += () =>
            {
                if (vm.ActiveProject == null) return;
                var eventsVm = new EventsViewModel(vm.ActiveProject.AbsoluteEventsPath);
                var window = new EventsWindow { DataContext = eventsVm };
                window.Show(this);
            };
                vm.ItemsOpenRequested += () =>
                {
                    if (vm.ActiveProject == null) return;
                    var window = new ItemsWindow
                    {
                        DataContext = new ItemsViewModel(vm.ActiveProject.AbsoluteItemsPath)
                    };
                    window.Show(this);
                };
            }
        };
    }

    private void OnLoaded(object? sender, RoutedEventArgs e)
    {
        var mapList = this.FindControl<ListBox>("MapList");
        if (mapList != null)
            mapList.DoubleTapped += OnMapListDoubleTapped;

        var recentMenu = this.FindControl<MenuItem>("RecentProjectsMenu");
        if (recentMenu != null)
            recentMenu.SubmenuOpened += OnRecentMenuOpened;
    }

    private void OnRecentMenuOpened(object? sender, RoutedEventArgs e)
    {
        if (sender is not MenuItem menu) return;
        if (DataContext is not EditorViewModel vm) return;

        menu.Items.Clear();

        var projects = vm.RecentProjects;
        if (!projects.Any())
        {
            menu.Items.Add(new MenuItem
            {
                Header = "Aucun projet récent",
                IsEnabled = false
            });
            return;
        }

        foreach (var recent in projects)
        {
            var item = new MenuItem { Header = $"{recent.Name}  —  {recent.Path}" };
            var captured = recent;
            item.Click += (_, _) => vm.OpenRecentProjectCommand.Execute(captured);
            menu.Items.Add(item);
        }
    }

    private void OnMapListDoubleTapped(object? sender, RoutedEventArgs e)
    {
        if (DataContext is not EditorViewModel vm) return;
        if (vm.MapBrowser?.SelectedMap is { } summary)
            vm.MapBrowser.OpenMapCommand.Execute(summary);
    }
}