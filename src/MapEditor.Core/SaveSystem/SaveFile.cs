namespace DungeonCrawler.RaylibGame;

public class LocationSave
{
    public string MapId { get; set; } = "";
    public int X { get; set; }
    public int Y { get; set; }
    public string Facing { get; set; } = "NORTH";
}

public class SaveFile
{
    public int Version { get; set; } = 1;
    public DateTime SavedAt { get; set; } = DateTime.Now;
    public string SlotName { get; set; } = "";
    public string HeroName { get; set; } = "";

    public LocationSave Location { get; set; } = new();

    // Futurs champs ajoutés ici sans casser les anciennes saves :
    // public PartySave?  Party      { get; set; }
    // public WorldSave?  WorldState { get; set; }
}