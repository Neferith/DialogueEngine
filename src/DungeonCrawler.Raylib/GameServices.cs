using DungeonCrawler.Core;
using DungeonCrawler.EventSystems;
using DungeonCrawler.Persistence;

namespace DungeonCrawler.RaylibGame;

public record GameServices(
    SaveManager SaveManager,
    EventSystem Events,
    EventScriptRegistry ScriptRegistry,
    ItemRegistry Items);