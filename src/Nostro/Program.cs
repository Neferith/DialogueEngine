using DungeonCrawler.EventSystems;
using DungeonCrawler.MapLoader;
using DungeonCrawler.Persistence;
using DungeonCrawler.RaylibGame;
using Nostro;

var config = NostroConfig.Create();
var saveManager = new SaveManager(config.SaveFolderName);

// ── Scripts custom Nostro ─────────────────────────────────────────────────────
var scriptRegistry = new EventScriptRegistry();
// scriptRegistry.Register(new MyCustomScript());

// ── EventSystem — chargé depuis JSON ─────────────────────────────────────────
var eventSystem = new EventSystem(scriptRegistry);
var eventLoader = new EventLoader();
eventLoader.LoadInto(eventSystem, config.EventsPath, scriptRegistry);

var itemLoader = new ItemLoader();
var itemRegistry = itemLoader.Load(config.ItemsPath);

// ── Lancement ─────────────────────────────────────────────────────────────────
var services = new GameServices(
    saveManager, 
    eventSystem, 
    scriptRegistry, 
    itemRegistry);
new GameScreenRunner(new MainMenuScreen(config, services), config).Run();