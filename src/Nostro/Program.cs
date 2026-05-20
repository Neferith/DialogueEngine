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

using DungeonCrawler.Core.Persist;
using DungeonCrawler.RaylibGame;
using Nostro;

var config = NostroConfig.Create();
var saveManager = new SaveManager(config.SaveFolderName);

new GameScreenRunner(
    new MainMenuScreen(config, saveManager),
    config).Run();