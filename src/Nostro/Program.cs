using DungeonCrawler.Core;
using DungeonCrawler.Core.Characters;
using DungeonCrawler.Core.Entities;
using DungeonCrawler.Core.Models;
using DungeonCrawler.Core.Systems;
using DungeonCrawler.MapLoader;
using DungeonCrawler.RaylibGame;

// ── Chargement de la map ──────────────────────────────────────────────────────

var loader = new MapFileLoader();
var loaded = loader.Load(
    Path.Combine("maps", "the_cells.map.json"),
    "modules");

// ── Party ─────────────────────────────────────────────────────────────────────

var spawnPos = loaded.PlayerSpawn ?? new GridPosition(1, 1);
var spawnFacing = loaded.PlayerFacing;

var party = new Party(spawnPos, spawnFacing, maxSize: 4);
party.TryAddMember(new PartyMember("Aria"));
party.TryAddMember(new PartyMember("Borin"));

// ── Entités (hardcodées en attendant le système d'entités) ────────────────────

var entities = new EntitySystem();

// ── Lancement ─────────────────────────────────────────────────────────────────

var runner = new DungeonRunner(loaded.Map, party, entities);
var turns = new TurnManager(runner, entities);

new RaylibGameRunner(runner, turns).Run("Nostro");