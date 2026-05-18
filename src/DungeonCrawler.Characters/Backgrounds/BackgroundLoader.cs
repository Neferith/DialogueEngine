using System.Text.Json;
using System.Text.Json.Serialization;

namespace DungeonCrawler.Characters.Backgrounds;

public class BackgroundLoader
{
    private static readonly JsonSerializerOptions _options = new()
    {
        PropertyNameCaseInsensitive = true,
        Converters = { new JsonStringEnumConverter() }
    };

    public List<BackgroundType> LoadAll(string backgroundsPath)
    {
        if (!File.Exists(backgroundsPath)) return new();
        try
        {
            var json = File.ReadAllText(backgroundsPath);
            return JsonSerializer.Deserialize<List<BackgroundType>>(json, _options) ?? new();
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"[BackgroundLoader] Erreur : {ex.Message}");
            return new();
        }
    }

    /// <summary>Filtre les backgrounds disponibles selon la taille du personnage.</summary>
    public static IEnumerable<Background> FilterFor(
        IEnumerable<BackgroundType> types,
        Creation.CharacterSize size) =>
        types.SelectMany(t => t.Backgrounds)
             .Where(b => b.Requirement?.IsMet(size) ?? true);
}
