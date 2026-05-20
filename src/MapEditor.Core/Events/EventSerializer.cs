using System.Text.Json;

namespace MapEditor.Core.Events;

public static class EventSerializer
{
    private static readonly JsonSerializerOptions _options = new()
    {
        WriteIndented = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        PropertyNameCaseInsensitive = true
    };

    public static EventFile Load(string path)
    {
        if (!File.Exists(path)) return new EventFile();
        try
        {
            return JsonSerializer.Deserialize<EventFile>(
                File.ReadAllText(path), _options) ?? new EventFile();
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"[EventSerializer] Erreur {path} : {ex.Message}");
            return new EventFile();
        }
    }

    public static void Save(string path, EventFile file)
    {
        Directory.CreateDirectory(Path.GetDirectoryName(path)!);
        File.WriteAllText(path, JsonSerializer.Serialize(file, _options));
    }
}