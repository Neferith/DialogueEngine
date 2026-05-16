using System.Text.Json;
using MapEditor.Core.Models;

namespace MapEditor.Core.Serialization;

public class MapSerializer
{
    private static readonly JsonSerializerOptions _writeOptions = new()
    {
        WriteIndented           = true,
        PropertyNamingPolicy    = JsonNamingPolicy.CamelCase,
        DefaultIgnoreCondition  = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull
    };

    private static readonly JsonSerializerOptions _readOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        ReadCommentHandling         = JsonCommentHandling.Skip
    };

    public string Serialize(MapFile map)   => JsonSerializer.Serialize(map, _writeOptions);
    public void   Save(MapFile map, string path) => File.WriteAllText(path, Serialize(map));

    public MapFile? Deserialize(string json)       => JsonSerializer.Deserialize<MapFile>(json, _readOptions);
    public MapFile? Load(string path)              => Deserialize(File.ReadAllText(path));
}
