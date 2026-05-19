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
- [x] `BiomeTextures` — record pour les chemins de textures par biome
- [x] `SaveFile` + `CharacterSaveData` + `InjurySaveData` (namespace `Core.Persist`)
- [x] `SaveManager` — 5 slots, %AppData%

### DungeonCrawler.Characters ← nouveau projet
- [x] `CharacterAttribute`, `CharacterAttributes`, `AttributesModifier`
- [x] `CharacterGender`, `CharacterSize`, `CharacterWeight`, `CharacterSensitivity`
- [x] Chaîne de filtrage : Genre → Taille → Poids → Sensibilité
- [x] `Background`, `BackgroundType`, `BackgroundLoader`, `CharacterRules`
- [x] `Skill`, `CharacterSkills`
- [x] `Injury` — hiérarchie sealed Physical/Mental/Energy avec Severity
- [x] `Character` + stats dérivées (MaxHp, QAm, QAc, QDp, QDe) + quotients finaux
- [x] `CharacterState` — WithDamage, WithHeal, WithInjury
- [x] `CharacterBuilder` — IsLastStep, IsComplete, Build()
- [x] Tests : AttributeTests, CharacterStatsTests, CreationChainTests, CharacterBuilderTests

### DungeonCrawler.Raylib
- [x] `DungeonRenderer` — rendu 3D style M&M (murs, sol, plafond, portes, entités)
- [x] Textures par biome (`LoadTextureSet`)
- [x] Animations de déplacement (avancer, reculer, tourner, strafer)
- [x] HUD (position, orientation, tour, members, prompt interaction)
- [x] `IGameScreen` — interface machine à états
- [x] `GameScreenRunner` — boucle Raylib + transitions d'écrans
- [x] `Raylib.SetExitKey(KeyboardKey.Null)` — Escape géré par les écrans
- [x] `PlayingScreen` — gameplay donjon, F5 quicksave, I → StatsScreen
- [x] `MainMenuScreen` — Nouvelle partie / Charger / Quitter
- [x] `SlotSelectScreen` — sélection de slot (5 slots)
- [x] `CharacterCreationScreen` — flow complet 6 étapes avec cartes sélectionnables
- [x] `StatsScreen` — liste party + détail (attributs, combat, HP bar, compétences, blessures)
- [x] `FantasyUI` — Button, Panel, TextInput, Title, Label, SelectableCard, HandleTextInput
- [x] `CampaignConfig` + `RaylibColorScheme` (record) — config par campagne
- [x] `ActiveSave(SaveManager, SlotIndex, HeroName, List<Character>)` — contexte en cours de partie
- [x] `CharacterMapper` — Character ↔ CharacterSaveData

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
- [x] `CampaignProject` — fichier projet `.campaign.json` + `CharacterRulesPath`
- [x] `MapSummary` — résumé léger pour le navigateur
- [x] `CharacterRulesFile` — BackgroundTypeData, BackgroundData, SkillData
- [x] `CharacterRulesSerializer`

### MapEditor.Avalonia
- [x] Ouverture de projet campagne (`.campaign.json`)
- [x] Projets récents (stockés dans %AppData%)
- [x] Palette tiles + entités par biome (onglets)
- [x] Navigateur de maps (onglet Maps, double-clic pour ouvrir)
- [x] Canvas interactif — peinture, effacement, sélection
- [x] Panneau propriétés — tile (walkable, transition) + entité (orientation, props)
- [x] Création de transition avec porte retour automatique
- [x] Création de nouveau biome (dossier + module.json template)
- [x] Menu **Personnages → Règles de création** → `CharacterRulesWindow` (popup)
- [x] `CharacterRulesViewModel`, `BackgroundTypeViewModel`, `BackgroundEditorViewModel`
- [x] `SkillsViewModel`, `SkillViewModel`
- [x] Auto-création de `character_rules.json` si absent

### Nostro
- [x] Campagne dark fantasy style M&M3
- [x] `NostroConfig` — palette DA, police MedievalSharp, chemins
- [x] Map `the_cells` + map `level_3` avec transition bidirectionnelle
- [x] Module `stone_dungeon` + module `cave` (en cours)
- [x] `rules/character_rules.json` — 2 types (Profession + Antécédent), 12 skills
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
- [ ] Police pixelisée dans Raylib (charger à 256px dans `FantasyUI.Init`)
- [ ] `DungeonCrawler.Core.Persist` → futur projet `DungeonCrawler.Persistence`
- [ ] Erreurs tests `DungeonCrawler.MapLoader.Tests` (renommage CharacterAttribute)
- [ ] Renommage de la solution (de DialogueEngine2)

---

## 📋 Court terme (Nostro)

### Contenu
- [ ] Dessiner les maps de la campagne Nostro
- [ ] Module `cave` — textures et tiles spécifiques
- [ ] Transitions testées sur plusieurs maps enchaînées

### Systèmes
- [ ] Écran de pause (Escape depuis PlayingScreen)
- [ ] Retour au menu depuis le jeu
- [ ] Entités depuis le JSON (NPC, items via `EntityPlacement`)
- [ ] Système de factions NPC
- [ ] Branchement DialogueEngine sur les NPC
- [ ] Persistance état du monde (portes ouvertes, items ramassés)

### UI / UX
- [ ] Bouton Save dans le panneau UI droit

---

## 🎯 Moyen terme

### DungeonCrawler
- [ ] Système de combat tour par tour (1d20 + quotient vs 1d20 + quotient)
- [ ] Blessures sur grandes marges
- [ ] Inventaire de la party
- [ ] Équipement (armes, armures) → bonus quotients
- [ ] Compétences actives en combat
- [ ] Leveling (XP, distribution de points)
- [ ] Recrutement de membres dans l'équipe

### Éditeur
- [ ] Génération de squelette de projet campagne depuis l'éditeur
- [ ] Zoom sur le canvas
- [ ] Undo/Redo

### Infrastructure
- [ ] CI/CD + NuGet pour les libs réutilisables
- [ ] Documentation XML sur l'API publique

---

## 🌟 Vision long terme

- [ ] Port Godot 4 (remplacer DungeonCrawler.Raylib par un renderer Godot)
- [ ] Multiples campagnes indépendantes utilisant le même moteur
- [ ] Éditeur générant le squelette d'une campagne (`.csproj`, assets, config)
- [ ] Système de scripting embarqué pour les conséquences de dialogue
- [ ] Arbre de dialogue visuel dans DialogueEngine.Editor