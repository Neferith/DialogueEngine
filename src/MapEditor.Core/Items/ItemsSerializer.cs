using System.Text.Json;

namespace MapEditor.Core.Items;

public static class ItemsSerializer
{
    private static readonly JsonSerializerOptions _options = new()
    {
        WriteIndented = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        PropertyNameCaseInsensitive = true
    };

    public static ItemsFile Load(string path)
    {
        if (!File.Exists(path)) return new ItemsFile();
        try
        {
            return JsonSerializer.Deserialize<ItemsFile>(
                File.ReadAllText(path), _options) ?? new ItemsFile();
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"[ItemsSerializer] Erreur : {ex.Message}");
            return new ItemsFile();
        }
    }

    public static void Save(string path, ItemsFile file)
    {
        Directory.CreateDirectory(Path.GetDirectoryName(path)!);
        File.WriteAllText(path, JsonSerializer.Serialize(file, _options));
    }
}