namespace DungeonCrawler.Core;

public class ItemRegistry
{
    private readonly Dictionary<string, ItemDefinition> _items = new();

    public void Register(ItemDefinition item)
    {
        if (_items.ContainsKey(item.Id))
            throw new InvalidOperationException(
                $"Item déjà enregistré : {item.Id}");
        _items[item.Id] = item;
    }

    public ItemDefinition? Get(string id) =>
        _items.TryGetValue(id, out var item) ? item : null;

    public IReadOnlyCollection<ItemDefinition> All => _items.Values;

    public bool Contains(string id) => _items.ContainsKey(id);
}