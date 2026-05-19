namespace DungeonCrawler.RaylibGame;

using DungeonCrawler.Characters.Models;
using DungeonCrawler.Core.Persist;

public record ActiveSave(
    SaveManager Manager, 
    int SlotIndex, 
    string HeroName,
    List<Character> Characters);