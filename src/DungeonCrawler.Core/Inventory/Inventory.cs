namespace DungeonCrawler.Core;

public class Inventory
{
    private readonly Dictionary<string, int> _items = new();
    public int? MaxSlots { get; init; }

    public IReadOnlyDictionary<string, int> Items => _items;
    public bool IsEmpty => _items.Count == 0;
    public bool IsFull => MaxSlots.HasValue && _items.Count >= MaxSlots.Value;
    public int SlotCount => _items.Count;

    public bool Contains(string itemId) => _items.ContainsKey(itemId);
    public int GetQuantity(string itemId) => _items.GetValueOrDefault(itemId, 0);

    public bool Add(string itemId, int quantity = 1)
    {
        if (IsFull && !Contains(itemId)) return false;
        _items[itemId] = GetQuantity(itemId) + quantity;
        return true;
    }

    public bool Remove(string itemId, int quantity = 1)
    {
        if (!Contains(itemId)) return false;
        var newQty = _items[itemId] - quantity;
        if (newQty <= 0) _items.Remove(itemId);
        else _items[itemId] = newQty;
        return true;
    }

    public void Clear() => _items.Clear();
}