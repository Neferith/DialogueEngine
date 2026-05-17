using DungeonCrawler.Core.Models;
using MapEditor.Core.Modules;

namespace DungeonCrawler.MapLoader;

public static class ModuleTexturesConverter
{
    public static BiomeTextures? Convert(ModuleDefinition module)
    {
        if (module.Textures == null || module.ModuleDirectory == null)
            return null;

        string Abs(string? rel) =>
            string.IsNullOrEmpty(rel) ? "" : Path.Combine(module.ModuleDirectory, rel);

        return new BiomeTextures(
            Wall: Abs(module.Textures.Wall),
            Floor: Abs(module.Textures.Floor),
            Ceiling: Abs(module.Textures.Ceiling),
            DoorClosed: Abs(module.Textures.DoorClosed),
            DoorOpen: Abs(module.Textures.DoorOpen));
    }
}