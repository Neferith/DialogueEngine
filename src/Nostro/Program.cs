using DungeonCrawler.Core;
using DungeonCrawler.Core.Characters;
using DungeonCrawler.Core.Entities;
using DungeonCrawler.Core.Entities.Behaviors;
using DungeonCrawler.Core.Models;
using DungeonCrawler.Core.Systems;
using DungeonCrawler.MapLoader;
using DungeonCrawler.RaylibGame;

// ── Chemins ───────────────────────────────────────────────────────────────────

var mapsPath = "maps";
var modulesPath = "modules";

// ── Chargement de la map ──────────────────────────────────────────────────────

var loader = new MapFileLoader();
var loaded = loader.Load(Path.Combine(mapsPath, "the_cells.map.json"), modulesPath);

// ── Party ─────────────────────────────────────────────────────────────────────

var party = new Party(
    loaded.PlayerSpawn ?? new GridPosition(1, 1),
    loaded.PlayerFacing,
    maxSize: 4);

party.TryAddMember(new PartyMember("Aria"));
party.TryAddMember(new PartyMember("Borin"));

// ── Entités (hardcodées en attendant le système d'entités) ────────────────────

var entities = new EntitySystem();

// ── Session ───────────────────────────────────────────────────────────────────

var runner = new DungeonRunner(loaded.Map, party, entities);
var turns = new TurnManager(runner, entities);
var session = new DungeonSession(loaded, runner, turns, loader, mapsPath, modulesPath);
    
// ── Lancement ─────────────────────────────────────────────────────────────────

new RaylibGameRunner(session).Run("Nostro");