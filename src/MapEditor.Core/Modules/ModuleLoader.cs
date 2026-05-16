using System.Text.Json;
using System.Text.Json.Serialization;

namespace MapEditor.Core.Modules;

public class ModuleDefinition
{
    public string  Id                   { get; set; } = "";
    public string  Name                 { get; set; } = "";
    /// <summary>Path to the tiles spritesheet, relative to the module folder.</summary>
    public string? TilesTexturePath     { get; set; }
    /// <summary>Path to the entities spritesheet, relative to the module folder.</summary>
    public string? EntitiesTexturePath  { get; set; }
    /// <summary>Pixel size of one sprite (assumed square).</summary>
    public int     SpriteSize           { get; set; } = 16;

    public List<TileTypeDefinition>   TileTypes   { get; set; } = new();
    public List<EntityTypeDefinition> EntityTypes { get; set; } = new();

    /// <summary>Absolute path to the module folder — set at load time, not serialized.</summary>
    [JsonIgnore]
    public string? ModuleDirectory { get; set; }

    public TileTypeDefinition?   FindTileType(string id)   => TileTypes.FirstOrDefault(t => t.Id == id);
    public EntityTypeDefinition? FindEntityType(string id) => EntityTypes.FirstOrDefault(e => e.Id == id);
}

public class ModuleLoader
{
    private static readonly JsonSerializerOptions _options = new()
    {
        PropertyNameCaseInsensitive = true,
        ReadCommentHandling = JsonCommentHandling.Skip
    };

    /// <summary>Scans every sub-folder of <paramref name="modulesPath"/> for a module.json.</summary>
    public List<ModuleDefinition> LoadAll(string modulesPath)
    {
        var modules = new List<ModuleDefinition>();

        if (!Directory.Exists(modulesPath))
            return modules;

        foreach (var dir in Directory.GetDirectories(modulesPath))
        {
            var moduleFile = Path.Combine(dir, "module.json");
            if (!File.Exists(moduleFile)) continue;

            try
            {
                var json   = File.ReadAllText(moduleFile);
                var module = JsonSerializer.Deserialize<ModuleDefinition>(json, _options);
                if (module == null) continue;

                module.ModuleDirectory = dir;
                modules.Add(module);
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"[ModuleLoader] Failed to load {moduleFile}: {ex.Message}");
            }
        }

        return modules;
    }
}
