using Avalonia;
using CommunityToolkit.Mvvm.ComponentModel;
using MapEditor.Core.Items;

namespace MapEditor.Avalonia.ViewModels.Items;

public partial class ItemViewModel : ObservableObject
{
    [ObservableProperty] private string _id = "";
    [ObservableProperty] private string _title = "";
    [ObservableProperty] private string _description = "";
    [ObservableProperty] private string _type = "Other";
    [ObservableProperty] private bool _stackable = false;
    [ObservableProperty] private int _maxStack = 1;
    [ObservableProperty] private string _spritePath = "";

    public static IReadOnlyList<string> AvailableTypes =>
        ["Other", "Quest", "Equipment"];

    public ItemViewModel() { }

    public ItemViewModel(ItemData data)
    {
        _id = data.Id;
        _title = data.Title;
        _description = data.Description;
        _type = data.Type;
        _stackable = data.Stackable;
        _maxStack = data.MaxStack;
        _spritePath = data.SpritePath;
    }

    public ItemData ToData() => new()
    {
        Id = Id.Trim(),
        Title = Title.Trim(),
        Description = Description.Trim(),
        Type = Type,
        Stackable = Stackable,
        MaxStack = MaxStack,
        SpritePath = SpritePath.Trim()
    };
}