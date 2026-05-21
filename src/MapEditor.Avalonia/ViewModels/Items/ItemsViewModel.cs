using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MapEditor.Core.Items;

namespace MapEditor.Avalonia.ViewModels.Items;

public partial class ItemsViewModel : ObservableObject
{
    private readonly string _filePath;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(RemoveItemCommand))]
    private ItemViewModel? _selectedItem;

    public ObservableCollection<ItemViewModel> Items { get; } = new();

    public ItemsViewModel(string filePath)
    {
        _filePath = filePath;
        Load();
    }

    private void Load()
    {
        var file = ItemsSerializer.Load(_filePath);
        Items.Clear();
        foreach (var item in file.Items)
            Items.Add(new ItemViewModel(item));
        SelectedItem = Items.FirstOrDefault();
    }

    [RelayCommand]
    private void AddItem()
    {
        var vm = new ItemViewModel { Id = "new_item", Title = "Nouvel item" };
        Items.Add(vm);
        SelectedItem = vm;
    }

    [RelayCommand(CanExecute = nameof(CanRemoveItem))]
    private void RemoveItem()
    {
        if (SelectedItem == null) return;
        Items.Remove(SelectedItem);
        SelectedItem = Items.LastOrDefault();
    }

    private bool CanRemoveItem() => SelectedItem != null;

    [RelayCommand]
    private void Save()
    {
        var file = new ItemsFile
        {
            Items = Items.Select(i => i.ToData()).ToList()
        };
        ItemsSerializer.Save(_filePath, file);
    }
}