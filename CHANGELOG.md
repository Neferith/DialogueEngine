# Changelog

Format basé sur [Keep a Changelog](https://keepachangelog.com/fr/1.0.0/).

---

## [Unreleased]

### Prévu
- Entités (NPC, items) chargées depuis le JSON
- Système de factions NPC
- Branchement DialogueEngine sur les NPC
- Écran de pause + retour au menu depuis le jeu
- Bouton Save dans l'UI du jeu
- Système de combat (1d20 + quotient)
- EventLoader (JSON → EventSystem)
- Éditeur d'events dans MapEditor

---

## [0.7.0] — Items + Events toolset + Écran de pause
 
### Ajouté
- `DungeonCrawler.Core.Tests` — nouveau projet de tests
  - `InventoryTests` — 13 tests
  - `ItemRegistryTests` — 8 tests
- `Inventory` dans `DungeonCrawler.Core` — mutable, `MaxSlots?`, `Add/Remove → bool`
- `ItemDefinition`, `ItemType` (sealed: Other/Quest/Equipment), `StackRules` dans `DungeonCrawler.Core`
- `ItemRegistry` dans `DungeonCrawler.Core` — catalogue runtime
- `Tile.FloorInventory` — chaque tile a un inventaire au sol
- `ItemLoader` dans `DungeonCrawler.MapLoader` — charge `items.json` → `ItemRegistry`
- `EventLoader` dans `DungeonCrawler.EventSystems` — charge events depuis JSON
- `PauseOverlay` dans `DungeonCrawler.Raylib` — Reprendre / Sauvegarder / Menu principal
- `GameServices` étendu avec `ItemRegistry`
- `EventData`, `EventConditionData`, `EventEffectData`, `EventFile`, `EventSerializer` dans `MapEditor.Core`
- `ItemsFile`, `ItemData`, `ItemsSerializer` dans `MapEditor.Core`
- `ScriptDefinition`, `ScriptParamDefinition`, `ScriptsFile` dans `MapEditor.Core`
- `CampaignProject.EventsPath`, `ItemsPath` + chemins absolus
- `EventsWindow` dans `MapEditor.Avalonia` — liste fichiers events, éditeur events+effets
- `ItemsWindow` dans `MapEditor.Avalonia` — éditeur items (id, titre, description, type, stack, sprite)
- Overlays canvas `MapCanvasControl` — porte pixel art (coin bas-droit), fiole rouge (coin bas-gauche)
- Items au sol dans panneau propriétés tile (+ / - avec ComboBox + NumericUpDown)
- `events/maps/the_cells.events.json` dans Nostro (migré depuis Program.cs)
- `items/items.json` dans Nostro (3 items)
### Modifié
- `TileData` — ajout `List<TileItemData> Items`
- `MapFileLoader.BuildLoadedMap()` — charge les items des tiles → `FloorInventory`
- `PlayingScreen` — bloque input pendant pause (`PauseOverlay.IsActive`)
- `StatsScreen` — reçoit `GameServices`
- `Program.cs` Nostro — utilise `EventLoader` + `ItemLoader`, plus d'events hardcodés
### Dans [Unreleased] — modifier "Prévu" : supprimer
- "EventLoader (JSON → EventSystem)"
- "Éditeur d'events dans MapEditor"
### Dans [Unreleased] — modifier "Prévu" : ajouter
- Sprites billboard items dans DungeonRenderer
- PickupOverlay — menu ramassage
- GiveItemAction complet
- WorldState.TileInventoryOverrides
- Affichage inventaire dans StatsScreen

## [0.6.0] — Events + Dialogue narratif

### Ajouté
- `DungeonCrawler.Persistence` — nouveau projet (extrait de Core.Persist)
  - `SaveFile`, `LocationSave`, `CharacterSaveData`, `InjurySaveData`
  - `NpcState` (Hostility, Affinity, IsAlive, IsRecruited)
  - `WorldState` (Flags, Variables, Npcs)
  - `SaveManager`
- `DungeonCrawler.EventSystems` — nouveau projet
  - `GameEvent`, `EventCondition`, `EventEffect { ScriptId, Params }`
  - `IEventScript`, `ScriptParameter`, `EventScriptContext` (API complète)
  - `EventScriptRegistry` avec 8 scripts built-in
  - `IGameAction` + 6 implémentations
  - `EventSystem` avec 6 triggers
- `GameServices(SaveManager, EventSystem, EventScriptRegistry)` — transite dans tous les écrans
- `DialogueOverlay` — overlay typewriter sur PlayingScreen
  - `BlocksInput = true` par défaut
  - `_justSkipped` flag (skip typewriter sans avancer)
  - `TryAdvance()` défensif avec try/catch
- `intro_dialogue.json` dans `Nostro/dialogues/`
- Event intro MapEnter one-shot (SetFlag + StartDialogue)

### Modifié
- `DungeonSession` — reçoit `EventSystem?` + `WorldState?`, expose `NotifyMapEntered()`
- `PlayingScreen` — input bloqué pendant dialogue (`dialogueBlocking`)
- `SaveFile` — ajout `WorldState`
- `ActiveSave` — ajout `WorldState`
- `MainMenuScreen`, `SlotSelectScreen`, `CharacterCreationScreen` → `GameServices`
- `Program.cs` Nostro — configuration `GameServices` + event hardcodé

---

## [0.5.0] — Système de personnages + Sauvegarde complète

### Ajouté
- `DungeonCrawler.Characters` — nouveau projet indépendant
  - `CharacterAttribute`, `CharacterAttributes`, `AttributesModifier`
  - Chaîne de filtrage : Genre → Taille → Poids → Sensibilité → Background
  - `Background`, `BackgroundType`, `BackgroundLoader`, `CharacterRules`
  - `Skill`, `CharacterSkills`
  - `Injury` — hiérarchie sealed Physical/Mental/Energy avec Severity
  - `Character` + stats dérivées + quotients finaux
  - `CharacterState`, `CharacterBuilder`
  - Tests : AttributeTests, CharacterStatsTests, CreationChainTests, CharacterBuilderTests
- `CharacterSaveData`, `InjurySaveData` dans `DungeonCrawler.Core.Persist`
- `SaveFile.Party` — liste de `CharacterSaveData`
- `CharacterMapper` dans `DungeonCrawler.Raylib` — Character ↔ CharacterSaveData
- `ActiveSave` étendu avec `List<Character> Characters`
- `CharacterCreationScreen` — flow complet 6 étapes avec cartes sélectionnables
- `StatsScreen` — liste party + détail (attributs, combat, HP bar, compétences, blessures)
- `FantasyUI.SelectableCard` — carte cliquable avec état sélectionné/hover
- `CharacterRulesFile`, `CharacterRulesSerializer` dans `MapEditor.Core`
- `CharacterRulesViewModel` + views dans `MapEditor.Avalonia`
- Menu **Personnages → Règles de création** → `CharacterRulesWindow` dans l'éditeur
- `rules/character_rules.json` dans Nostro (2 types, 12 skills)

### Modifié
- `CharacterCreationScreen` — remplace l'ancien écran (saisie nom uniquement)
- `PlayingScreen` — ajout `I` → StatsScreen, `_nextScreen` pattern, `F5` quicksave complet
- `GameScreenRunner.Run()` — `SetExitKey(KeyboardKey.Null)`
- `ActiveSave` — ajout `List<Character> Characters`
- `CampaignConfig` — ajout `CharacterRulesPath`
- `CampaignProject` — ajout `CharacterRulesPath` + `AbsoluteCharacterRulesPath`

---

## [0.4.0] — Campagne Nostro + Menu principal

### Ajouté
- `IGameScreen` — interface machine à états d'écrans
- `GameScreenRunner` — boucle Raylib avec transitions entre écrans
- `PlayingScreen` — gameplay donjon (extraction de RaylibGameRunner)
- `MainMenuScreen` — Nouvelle partie / Charger / Quitter
- `SlotSelectScreen` — sélection de slot (5 slots, infos hero + date)
- `CharacterCreationScreen` — saisie du nom du héros (v1)
- `FantasyUI` — helpers dark fantasy (Button, Panel, TextInput, Title, Label)
- `CampaignConfig` + `RaylibColorScheme` (record) — config par campagne
- `ActiveSave` — record regroupant SaveManager + slotIndex + heroName
- `SaveFile` / `SaveManager` dans `DungeonCrawler.Core` — sauvegarde JSON par slot (%AppData%)
- `NostroConfig` — palette DA Nostro, police MedievalSharp, chemins
- Police MedievalSharp chargée via `FantasyUI.Init()`
- F5 → quicksave de la position courante

### Modifié
- `RaylibGameRunner` → remplacé par `PlayingScreen` + `GameScreenRunner`
- `Nostro/Program.cs` — lance `MainMenuScreen` au lieu du jeu directement

---

## [0.3.0] — Éditeur de maps complet

### Ajouté
- `MapEditor.Core` — modèles maps, modules, sérialisation
- `MapEditor.Avalonia` — éditeur visuel complet
  - Ouverture de projet campagne (`.campaign.json`)
  - Projets récents (%AppData%/MapEditor/recent_projects.json)
  - Palette tiles + entités par biome (onglets Tiles / Entités / Maps)
  - Navigateur de maps avec double-clic pour ouvrir
  - Canvas interactif — peinture, effacement, sélection
  - Panneau propriétés contextuel (tile, entité, map)
  - Création de transition avec porte retour automatique
  - Création de nouveau biome (dossier + module.json template)
- `CampaignProject` — fichier `.campaign.json` pointant vers modules/ et maps/
- `RecentProjectsService` — historique des projets récents
- `MapBrowserViewModel` — navigateur de maps
- Module `stone_dungeon` — 5 types de tiles + 4 entités + textures

### Modifié
- `TileTypeDefinition` — ajout `TileTag`, `FloorType`, `CeilingType`
- `ModuleDefinition` — ajout `Textures`
- `ModuleTextures` — chemins wall, floor, ceiling, doorClosed, doorOpen

---

## [0.2.0] — Moteur donjon + renderer Raylib

### Ajouté
- `DungeonCrawler.Core` — DungeonMap, Party, MovementSystem, TurnManager, ViewBuilder, EntitySystem, BiomeTextures
- `DungeonCrawler.Raylib` — DungeonRenderer, animations, HUD
- `DungeonCrawler.MapLoader` — MapFileLoader (flip Y), LoadedMap, DungeonSession, tests
- `Nostro` — maps, textures stone dungeon, portes DoorOpen

### Modifié
- `TurnManager.HandleInteraction()` — porte → DoorOpen (ne disparaît plus)
- `DungeonRenderer` — gestion TileTag.DoorOpen

---

## [0.1.0] — DialogueEngine

### Ajouté
- `DialogueEngine.Core` — moteur de dialogue complet
- `DialogueEngine.Serialization` — JSON camelCase
- `DialogueEngine.Editor` — éditeur Avalonia (MVVM, factory-container)
- `Sample1`, `Sample2`
- 10 tests unitaires