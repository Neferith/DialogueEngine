using DungeonCrawler.Core;
using DungeonCrawler.Core.Characters;
using DungeonCrawler.Core.Entities;
using DungeonCrawler.Core.Entities.Behaviors;
using DungeonCrawler.Core.Models;
using DungeonCrawler.Core.Systems;
using DungeonCrawler.RaylibGame;

// ── Dungeon setup (hardcodé en attendant les maps éditeur) ────────────────────

var map = new DungeonMap(16, 16, "Catacombs - Level 1");
map.FillBorder();
for (int x = 1; x <= 7; x++) map.SetTile(x, 8, Tile.Wall());
for (int x = 9; x <= 14; x++) map.SetTile(x, 8, Tile.Wall());
for (int y = 10; y <= 13; y++) map.SetTile(4, y, Tile.Wall());
for (int x = 1; x <= 3; x++) map.SetTile(x, 10, Tile.Wall());
map.SetTile(4, 12, Tile.Door());
map.SetTile(11, 4, Tile.Wall()); map.SetTile(11, 5, Tile.Wall());
map.SetTile(12, 4, Tile.Wall()); map.SetTile(12, 5, Tile.Wall());
for (int y = 9; y <= 12; y++) map.SetTile(11, y, Tile.Wall());
map.SetTile(13, 13, Tile.StairsDown());

var entities = new EntitySystem();
entities.Add(new MonsterEntity("guard1", new GridPosition(8, 5), "Skeleton Guard",
             new PatrolBehavior(Direction.North)));
entities.Add(new MonsterEntity("rat1", new GridPosition(13, 3), "Giant Rat",
             new AggressiveBehavior(detectionRange: 5)));
entities.Add(new NpcEntity("sage1", new GridPosition(6, 11), "Old Sage",
             "dialogue_sage", Direction.East));
entities.Add(new ItemEntity("potion1", new GridPosition(3, 5), "health_potion", "Health Potion"));

var party = new Party(new GridPosition(4, 4), Direction.North, maxSize: 4);
party.TryAddMember(new PartyMember("Aria"));
party.TryAddMember(new PartyMember("Borin"));

var runner = new DungeonRunner(map, party, entities);
var turns = new TurnManager(runner, entities);

// ── Lancement ─────────────────────────────────────────────────────────────────

new RaylibGameRunner(runner, turns).Run("Nostro");