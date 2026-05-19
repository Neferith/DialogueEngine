using System.Text.Json;

namespace MapEditor.Core.Characters;

public static class CharacterRulesSerializer
{
    private static readonly JsonSerializerOptions _options = new()
    {
        WriteIndented = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        PropertyNameCaseInsensitive = true
    };

    public static CharacterRulesFile Load(string path)
    {
        if (!File.Exists(path)) return new CharacterRulesFile();
        try
        {
            var json = File.ReadAllText(path);
            return JsonSerializer.Deserialize<CharacterRulesFile>(json, _options)
                   ?? new CharacterRulesFile();
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"[CharacterRulesSerializer] Erreur : {ex.Message}");
            return new CharacterRulesFile();
        }
    }

    public static void Save(string path, CharacterRulesFile data)
    {
        Directory.CreateDirectory(Path.GetDirectoryName(path)!);
        File.WriteAllText(path, JsonSerializer.Serialize(data, _options));
    }
}