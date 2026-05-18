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

---

## [0.4.0] — Campagne Nostro + Menu principal

### Ajouté
- `IGameScreen` — interface machine à états d'écrans
- `GameScreenRunner` — boucle Raylib avec transitions entre écrans
- `PlayingScreen` — gameplay donjon (extraction de RaylibGameRunner)
- `MainMenuScreen` — Nouvelle partie / Charger / Quitter
- `SlotSelectScreen` — sélection de slot (5 slots, infos hero + date)
- `CharacterCreationScreen` — saisie du nom du héros
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
- `TileTypeDefinition` — ajout `TileTag`, `FloorType`, `CeilingType` (strings pour découplage)
- `ModuleDefinition` — ajout `Textures` (chemins relatifs par biome)
- `ModuleTextures` — chemins wall, floor, ceiling, doorClosed, doorOpen

---

## [0.2.0] — Moteur donjon + renderer Raylib

### Ajouté
- `DungeonCrawler.Core`
  - `DungeonMap`, `Tile`, `TileTag` (None, Door, DoorOpen, StairsUp, StairsDown…)
  - `Party`, `PartyMember`, `GridPosition`, `Direction`
  - `MovementSystem`, `TurnManager`, `ViewBuilder`
  - `EntitySystem` — MonsterEntity (PatrolBehavior, AggressiveBehavior), NpcEntity, ItemEntity
  - `BiomeTextures` — record pour chemins de textures
- `DungeonCrawler.Raylib`
  - `DungeonRenderer` — rendu 3D style M&M (scanline, textures, animations)
  - `RaylibGameRunner` — boucle de jeu Raylib
  - Animations de déplacement (avancer, reculer, tourner, strafer)
  - HUD (position, orientation, tour, prompt interaction)
- `DungeonCrawler.MapLoader`
  - `MapFileLoader` — charge `.map.json` + module → `DungeonMap`
  - Flip Y coordonnées éditeur → moteur (`height - 1 - y`)
  - `LoadedMap` — résultat du chargement (map, transitions, spawn, entités)
  - `DungeonSession` — transitions automatiques entre maps
  - `ModuleTexturesConverter` — `ModuleDefinition` → `BiomeTextures`
  - Tests unitaires (dimensions, tiles, spawn, transitions)
- `Nostro` — campagne jouable
  - Map `the_cells` + `level_3` avec transition bidirectionnelle
  - Textures stone dungeon (murs, sol, plafond, portes)
  - Portes avec état ouvert/fermé (`TileTag.DoorOpen`)

### Modifié
- `TurnManager.HandleInteraction()` — porte → `IsSolid=false`, `Tag=DoorOpen` (ne disparaît plus)
- `DungeonRenderer` — gestion `TileTag.DoorOpen` (texture porte ouverte + sol/plafond visible)

---

## [0.1.0] — DialogueEngine

### Ajouté
- `DialogueEngine.Core` — moteur de dialogue complet
  - `DialogueRunner` avec résolution conditionnelle nœuds + réponses
  - `nextNodeIds[]` — liste ordonnée de candidats avec fallback
  - `ScriptRegistry` — conditions et conséquences par clé string
  - `IDialogueContext` + `IVariableResolver` — substitution `{variable}`
  - `LocalizedText` — variantes conditionnelles (genre, race…)
  - Events `OnNodeEntered`, `OnDialogueEnd`, `OnDialogueCancelled`
  - `CancelConsequenceKey` par nœud
- `DialogueEngine.Serialization` — JSON camelCase
  - `LocalizedTextConverter` — format union string | TextVariant[]
- `DialogueEngine.Editor` — éditeur Avalonia (MVVM, factory-container)
  - Ouverture / sauvegarde / enregistrer sous
  - Validation inline, ordonnancement nœuds et réponses, duplication
- `Sample1` — conversation statique avec branches par rang
- `Sample2` — jeu vue de dessus, compétences, émotions NPC
- 10 tests unitaires