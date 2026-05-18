# Agenda

---

## ✅ Fait

### DialogueEngine
- [x] `DialogueEngine.Core` — moteur de dialogue complet
- [x] `DialogueEngine.Serialization` — JSON (camelCase, LocalizedTextConverter)
- [x] `DialogueEngine.Editor` — éditeur Avalonia (MVVM, factory-container)
- [x] `Sample1` — conversation statique avec branches par rang
- [x] `Sample2` — jeu vue de dessus, compétences, émotions NPC
- [x] 10 tests unitaires (xUnit + FluentAssertions)

### DungeonCrawler.Core
- [x] `DungeonMap` — grille 2D de tiles (IsSolid, TileTag, TextureId)
- [x] `Party` — position, orientation, membres
- [x] `MovementSystem` — déplacement case par case, collisions
- [x] `TurnManager` — séquencement party + entités, interactions
- [x] `ViewBuilder` — snapshot engine-agnostic (DungeonView)
- [x] `EntitySystem` — MonsterEntity, NpcEntity, ItemEntity + behaviors
- [x] `SaveFile` / `SaveManager` — sauvegarde JSON par slot (%AppData%)
- [x] `BiomeTextures` — record pour les chemins de textures par biome

### DungeonCrawler.Raylib
- [x] `DungeonRenderer` — rendu 3D style M&M (murs, sol, plafond, portes, entités)
- [x] Textures par biome (`LoadTextureSet`)
- [x] Animations de déplacement (avancer, reculer, tourner, strafer)
- [x] HUD (position, orientation, tour, members, prompt interaction)
- [x] `IGameScreen` — interface machine à états
- [x] `GameScreenRunner` — boucle Raylib + transitions d'écrans
- [x] `PlayingScreen` — gameplay donjon (ex-RaylibGameRunner)
- [x] `MainMenuScreen` — Nouvelle partie / Charger / Quitter
- [x] `SlotSelectScreen` — sélection de slot (5 slots)
- [x] `CharacterCreationScreen` — saisie du nom du héros
- [x] `FantasyUI` — helpers dark fantasy (Button, Panel, TextInput, Title, Label)
- [x] `CampaignConfig` + `RaylibColorScheme` — config par campagne
- [x] `ActiveSave` — contexte de sauvegarde en cours de partie
- [x] F5 → quicksave (position courante)

### DungeonCrawler.MapLoader
- [x] `MapFileLoader` — charge un `.map.json` + module → `DungeonMap`
- [x] Flip Y coordonnées éditeur → moteur
- [x] `LoadedMap` — résultat du chargement (map, transitions, entités, spawn)
- [x] `DungeonSession` — gestion des transitions entre maps en cours de partie
- [x] `ModuleTexturesConverter` — `ModuleDefinition` → `BiomeTextures`
- [x] Tests unitaires (dimensions, tiles, spawn, transitions)

### MapEditor.Core
- [x] `MapFile`, `TileData`, `EntityPlacement`, `MapTransition`
- [x] `ModuleDefinition`, `TileTypeDefinition`, `EntityTypeDefinition`
- [x] `ModuleLoader` — scan dossier modules/
- [x] `MapSerializer` — JSON camelCase
- [x] `CampaignProject` — fichier projet `.campaign.json`
- [x] `MapSummary` — résumé léger pour le navigateur

### MapEditor.Avalonia
- [x] Ouverture de projet campagne (`.campaign.json`)
- [x] Projets récents (stockés dans %AppData%)
- [x] Palette tiles + entités par biome (onglets)
- [x] Navigateur de maps (onglet Maps, double-clic pour ouvrir)
- [x] Canvas interactif — peinture, effacement, sélection
- [x] Panneau propriétés — tile (walkable, transition) + entité (orientation, props)
- [x] Création de transition avec porte retour automatique
- [x] Création de nouveau biome (dossier + module.json template)
- [x] Nouveau module `stone_dungeon` avec 5 types de tiles + 4 entités

### Nostro
- [x] Campagne dark fantasy style M&M3
- [x] `NostroConfig` — palette DA, police MedievalSharp, chemins
- [x] Map `the_cells` + map `level_3` avec transition bidirectionnelle
- [x] Module `stone_dungeon` + module `cave` (en cours)
- [x] Pipeline complet : éditeur → JSON → MapLoader → jeu

---

## 🔧 Dette technique connue

- [ ] `DialogueEngine.Editor` — texte localisé (variantes) non éditable dans l'UI
- [ ] `MapEditor.Avalonia` — édition des propriétés d'un biome (via JSON uniquement)
- [ ] `DungeonCrawler.Core` — entités depuis le JSON (NPC, items hardcodés dans Nostro)
- [ ] `DungeonSession` — pas encore de `DungeonSession` multi-maps avec historique
- [ ] `PlayingScreen` — pas de bouton Save dans l'UI (seulement F5)
- [ ] `SlotSelectScreen` — pas de suppression de slot
- [ ] `MapEditor.Avalonia` — PLAYER_SPAWN non unique (pas de validation)
- [ ] Transitions — pas de gestion de l'état ouvert/fermé des portes entre sessions

---

## 📋 Court terme (Nostro)

### Contenu
- [ ] Dessiner les maps de la campagne Nostro
- [ ] Module `cave` — textures et tiles spécifiques
- [ ] Transitions testées sur plusieurs maps enchaînées

### Systèmes
- [ ] Entités depuis le JSON (NPC, items via `EntityPlacement`)
- [ ] Système de factions NPC
- [ ] Branchement DialogueEngine sur les NPC
- [ ] Persistance état du monde (portes ouvertes, items ramassés)

### UI / UX
- [ ] Bouton Save dans le panneau UI droit
- [ ] Écran de pause (Escape)
- [ ] Retour au menu depuis le jeu

---

## 🎯 Moyen terme

### DungeonCrawler
- [ ] Système de combat tour par tour
- [ ] Inventaire de la party
- [ ] Stats des personnages (HP, caractéristiques)
- [ ] Effets de terrain (eau, lave, téléporteurs)

### Éditeur
- [ ] Génération de squelette de projet campagne depuis l'éditeur
- [ ] Zoom sur le canvas
- [ ] Undo/Redo

### Infrastructure
- [ ] Renommage de la solution
- [ ] CI/CD + NuGet pour les libs réutilisables
- [ ] Documentation XML sur l'API publique

---

## 🌟 Vision long terme

- [ ] Port Godot 4 (remplacer DungeonCrawler.Raylib par un renderer Godot)
- [ ] Multiples campagnes indépendantes utilisant le même moteur
- [ ] Éditeur générant le squelette d'une campagne (`.csproj`, assets, config)
- [ ] Système de scripting embarqué pour les conséquences de dialogue
- [ ] Arbre de dialogue visuel dans DialogueEngine.Editor