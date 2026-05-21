namespace MapEditor.Core.Items;

public class ItemsFile
{
    public List<ItemData> Items { get; set; } = new();
}

public class ItemData
{
    public string Id { get; set; } = "";
    public string Title { get; set; } = "";
    public string Description { get; set; } = "";
    public string Type { get; set; } = "Other";
    public bool Stackable { get; set; } = false;
    public int MaxStack { get; set; } = 1;
    public string SpritePath { get; set; } = "";
}