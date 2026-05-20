/*using DungeonCrawler.Core;
using DungeonCrawler.Core.Characters;
using DungeonCrawler.Core.Entities;
using DungeonCrawler.Core.Models;
using DungeonCrawler.Core.Systems;
using DungeonCrawler.MapLoader;
using DungeonCrawler.RaylibGame;
using Nostro;

// ── Config ────────────────────────────────────────────────────────────────────

var config = NostroConfig.Create();

// ── Chargement de la map ──────────────────────────────────────────────────────

var loader = new MapFileLoader();
var loaded = loader.Load(
    Path.Combine(config.MapsPath, "the_cells.map.json"),
    config.ModulesPath);

// ── Party ─────────────────────────────────────────────────────────────────────

var party = new Party(
    loaded.PlayerSpawn ?? new GridPosition(1, 1),
    loaded.PlayerFacing,
    maxSize: 4);

party.TryAddMember(new PartyMember("Aria"));
party.TryAddMember(new PartyMember("Borin"));

// ── Session ───────────────────────────────────────────────────────────────────

var entities = new EntitySystem();
var runner = new DungeonRunner(loaded.Map, party, entities);
var turns = new TurnManager(runner, entities);
var session = new DungeonSession(loaded, runner, turns, loader,
                                  config.MapsPath, config.ModulesPath);

// ── Lancement ─────────────────────────────────────────────────────────────────

var playingScreen = new PlayingScreen(session, config);
new GameScreenRunner(playingScreen, config).Run();*/

using DungeonCrawler.EventSystems;
using DungeonCrawler.Persistence;
using DungeonCrawler.RaylibGame;
using Nostro;

var config = NostroConfig.Create();

// ── Scripts custom Nostro ─────────────────────────────────────────────────────
var scriptRegistry = new EventScriptRegistry(); // built-ins déjà enregistrés

// Enregistrer ici les scripts custom de la campagne :
// scriptRegistry.Register(new MyCustomScript());

// ── EventSystem ───────────────────────────────────────────────────────────────
var eventSystem = new EventSystem(scriptRegistry);

// TODO : charger les events depuis JSON quand le toolset sera prêt
// Pour l'instant : event de test hardcodé
eventSystem.Register(new GameEvent
{
    Id = "intro",
    Trigger = EventTrigger.MapEnter,
    MapId = "the_cells",
    Condition = new EventCondition { FlagNotSet = "intro_played" },
    Effects =
    [
        new EventEffect { ScriptId = "SetFlag",      Params = new() { ["flagId"]    = "intro_played"    } },
        new EventEffect { ScriptId = "ShowMessage",  Params = new() { ["message"]   = "Bienvenue dans les cellules..." } }
    ]
});

// ── Services ──────────────────────────────────────────────────────────────────
var saveManager = new SaveManager(config.SaveFolderName);
var services = new GameServices(saveManager, eventSystem, scriptRegistry);

new GameScreenRunner(
    new MainMenuScreen(config, services),
    config).Run();