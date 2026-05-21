using DungeonCrawler.Core;
using Raylib_cs;

namespace DungeonCrawler.RaylibGame;

public class PickupOverlay
{
    private readonly CampaignConfig _config;
    private readonly ItemRegistry _itemRegistry;

    public bool IsActive { get; private set; }

    private Inventory? _tileInventory;
    private List<string> _itemIds = new();
    private int _selectedIndex;
    public Func<string, int, bool>? OnPickup;

    public PickupOverlay(CampaignConfig config, ItemRegistry itemRegistry)
    {
        _config = config;
        _itemRegistry = itemRegistry;
    }

    // ── API ───────────────────────────────────────────────────────────────────

    public void Open(Inventory tileInventory)
    {
        if (tileInventory.IsEmpty) return;
        _tileInventory = tileInventory;
        _itemIds = tileInventory.Items.Keys.ToList();
        _selectedIndex = 0;
        IsActive = true;
    }

    public void Close()
    {
        IsActive = false;
        _tileInventory = null;
        _itemIds.Clear();
    }

    // ── Update ────────────────────────────────────────────────────────────────

    public void Update()
    {
        if (!IsActive) return;

        if (Raylib.IsKeyPressed(KeyboardKey.Escape) ||
            Raylib.IsKeyPressed(KeyboardKey.G))
        {
            Close();
            return;
        }

        if (Raylib.IsKeyPressed(KeyboardKey.Up) || Raylib.IsKeyPressed(KeyboardKey.W))
            _selectedIndex = (_selectedIndex - 1 + _itemIds.Count) % _itemIds.Count;

        if (Raylib.IsKeyPressed(KeyboardKey.Down) || Raylib.IsKeyPressed(KeyboardKey.S))
            _selectedIndex = (_selectedIndex + 1) % _itemIds.Count;

        if (Raylib.IsKeyPressed(KeyboardKey.F) ||
            Raylib.IsKeyPressed(KeyboardKey.Enter))
            PickupSelected();
    }

    private void PickupSelected()
    {
        if (_tileInventory == null || _itemIds.Count == 0) return;

        var itemId = _itemIds[_selectedIndex];
        var qty = _tileInventory.GetQuantity(itemId);

        // 1. Retirer de la tile d'abord
        _tileInventory.Remove(itemId, qty);

        // 2. Notifier — si l'inventaire est plein, rollback
        if (OnPickup != null && !OnPickup.Invoke(itemId, qty))
        {
            _tileInventory.Add(itemId, qty); // rollback
            return;
        }

        _itemIds.Remove(itemId);
        if (_itemIds.Count == 0) { Close(); return; }
        _selectedIndex = Math.Min(_selectedIndex, _itemIds.Count - 1);
    }

    // ── Draw ──────────────────────────────────────────────────────────────────

    public void Draw(int w, int h)
    {
        if (!IsActive || _tileInventory == null) return;

        var colors = _config.Colors;

        float panW = 340f;
        float panH = 60f + _itemIds.Count * 52f;
        float panX = (w - panW) / 2f;
        float panY = (h - panH) / 2f;
        var rect = new Rectangle(panX, panY, panW, panH);

        FantasyUI.Panel(rect, colors);

        var title = "— Objets au sol —";
        var titleW = FantasyUI.MeasureText(title, 18f).X;
        FantasyUI.Title(title, panX + (panW - titleW) / 2f, panY + 12f, 18f, colors);

        float btnW = panW - 32f;
        float btnX = panX + 16f;
        float btnY = panY + 44f;

        for (int i = 0; i < _itemIds.Count; i++)
        {
            var id = _itemIds[i];
            var def = _itemRegistry.Get(id);
            var qty = _tileInventory.GetQuantity(id);

            var label = def != null
                ? qty > 1 ? $"{def.Title}  ×{qty}" : def.Title
                : qty > 1 ? $"{id}  ×{qty}" : id;

            var btnRect = new Rectangle(btnX, btnY + i * 52f, btnW, 44f);

            if (i == _selectedIndex)
            {
                Raylib.DrawRectangleRounded(btnRect, 0.2f, 8,
                    new Color(80, 60, 30, 200));
                Raylib.DrawRectangleRoundedLines(btnRect, 0.2f, 8, 2f, colors.Accent);
            }

            var labelW = FantasyUI.MeasureText(label, 16f).X;
            FantasyUI.Label(label,
                btnX + (btnW - labelW) / 2f,
                btnY + i * 52f + 13f,
                16f, colors);
        }

        FantasyUI.Label("↑↓ Sélectionner   F/Entrée Ramasser   G/Échap Fermer",
            panX + 8f, panY + panH - 20f, 11f, colors,
            colorOverride: colors.TextMuted);
    }
}