using System.Text.Json;

namespace DungeonCrawler.Persistence;

public class SaveManager
{
    private const int MaxSlots = 5;
    private readonly string _savesPath;

    private static readonly JsonSerializerOptions _writeOptions = new()
    {
        WriteIndented = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };
    private static readonly JsonSerializerOptions _readOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    public SaveManager(string campaignName)
    {
        _savesPath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            campaignName, "saves");
        Directory.CreateDirectory(_savesPath);
    }

    // ── Slots ─────────────────────────────────────────────────────────────────

    /// <summary>
    /// Retourne un tableau de MaxSlots entrées.
    /// null = slot vide.
    /// </summary>
    public SaveFile?[] GetAllSlots()
    {
        var slots = new SaveFile?[MaxSlots];
        for (int i = 0; i < MaxSlots; i++)
            slots[i] = Load(i);
        return slots;
    }

    public SaveFile? Load(int slot)
    {
        var path = SlotPath(slot);
        if (!File.Exists(path)) return null;
        try
        {
            var json = File.ReadAllText(path);
            return JsonSerializer.Deserialize<SaveFile>(json, _readOptions);
        }
        catch { return null; }
    }

    public void Save(int slot, SaveFile data)
    {
        data.SavedAt = DateTime.Now;
        File.WriteAllText(SlotPath(slot),
            JsonSerializer.Serialize(data, _writeOptions));
    }

    public void Delete(int slot)
    {
        var path = SlotPath(slot);
        if (File.Exists(path)) File.Delete(path);
    }

    public bool SlotExists(int slot) => File.Exists(SlotPath(slot));

    // ── Helpers ───────────────────────────────────────────────────────────────

    private string SlotPath(int slot) =>
        Path.Combine(_savesPath, $"slot_{slot}.json");
}