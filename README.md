# DialogueEngine2 *(nom temporaire)*

Ensemble d'outils et de moteurs en C# .NET 8 pour créer des dungeon crawlers RPG style Might & Magic.  
Inclut un moteur de dialogue, un moteur de donjon, un éditeur de maps visuel, et une campagne jouable (**Nostro**).

---

## Structure de la solution

```
src/
├── DialogueEngine.Core/          Moteur de dialogue — aucune dépendance UI
├── DialogueEngine.Serialization/ Lecture/écriture JSON des dialogues
├── DialogueEngine.Editor/        Éditeur de dialogues (Avalonia 11)
├── Sample1/                      Démo dialogue — conversation NES-style
├── Sample2/                      Démo dialogue — jeu vue de dessus
│
├── DungeonCrawler.Core/          Moteur donjon pur (maps, party, entités, Inventory, Items)
├── DungeonCrawler.Persistence/   DTOs de sauvegarde (SaveFile, WorldState, NpcState...)
├── DungeonCrawler.Characters/    Système RPG personnages (indépendant de Core)
├── DungeonCrawler.EventSystems/  Système d'events, scripts, actions
├── DungeonCrawler.Raylib/        Renderer Raylib + système d'écrans + sauvegarde
├── DungeonCrawler.MapLoader/     Pont éditeur ↔ moteur (chargement maps JSON)
│
├── MapEditor.Core/               Modèles de maps et modules (data, sérialisation)
├── MapEditor.Avalonia/           Éditeur visuel de maps (Avalonia 11)
│
└── Nostro/                       Campagne jouable — donjon dark fantasy

tests/
├── DialogueEngine.Core.Tests/        Tests moteur dialogue (xUnit + FluentAssertions)
├── DungeonCrawler.MapLoader.Tests/   Tests chargement de maps
├── DungeonCrawler.Characters.Tests/  Tests système de personnages
└── DungeonCrawler.Core.Tests/` dans structure tests
```

---

## Prérequis

- .NET 8 SDK
- Visual Studio 2022 17.8+ ou Rider 2023.3+

---

## Lancer les projets

```bash
# Jeu (campagne Nostro)
dotnet run --project src/Nostro

# Éditeur de maps + personnages
dotnet run --project src/MapEditor.Avalonia

# Éditeur de dialogues
dotnet run --project src/DialogueEngine.Editor

# Démos dialogue
dotnet run --project src/Sample1
dotnet run --project src/Sample2

# Tests
dotnet test
```

---

## Architecture

### Règle de dépendance

```
DialogueEngine.Core
    ↑
DialogueEngine.Serialization
    ↑
DialogueEngine.Editor

DungeonCrawler.Persistence   ← DTOs purs, aucune dépendance domaine
DungeonCrawler.Characters    ← RPG pur, aucune dépendance externe
DungeonCrawler.Core          ← moteur pur, aucune dépendance externe
    ↑
DungeonCrawler.EventSystems  ← ref Core + Persistence + Characters
    ↑
DungeonCrawler.MapLoader     ← ref Core + MapEditor.Core + EventSystems
DungeonCrawler.Raylib        ← ref tout sauf MapEditor.Avalonia
    ↑
MapEditor.Core    MapEditor.Avalonia
    ↑
Nostro  ←  tout assembler ici (Exe)
```

`DungeonCrawler.Core`, `Persistence` et `Characters` ne se connaissent pas.  
`EventSystems` fait le lien entre les trois.  
`Nostro` est le seul projet Exe du jeu — chaque campagne est son propre Exe.

---

## Moteur de donjon (DungeonCrawler.Core)

- **`DungeonMap`** — grille sparse de tiles (`IsSolid`, `TileTag`, `TextureId`)
- **`Party`** — position + orientation + membres
- **`MovementSystem`** — déplacement case par case, collisions
- **`TurnManager`** — séquencement party → entités, gestion interactions
- **`ViewBuilder`** — snapshot engine-agnostic de ce que la party voit
- **`EntitySystem`** — monstres, NPC, items sur la map

---

## Système de sauvegarde (DungeonCrawler.Persistence)

- **`SaveFile`** — version, slot, héros, position, party, WorldState
- **`WorldState`** — Flags (one-shot events), Variables (compteurs), Npcs (états NPC)
- **`SaveManager`** — 5 slots JSON dans `%AppData%/{campaign}/saves/`

---

## Système de personnages (DungeonCrawler.Characters)

- **`CharacterAttribute` / `CharacterAttributes` / `AttributesModifier`** — 4 stats
- **Chaîne de création** : Genre → Taille → Poids → Sensibilité → Background (filtrage progressif)
- **Stats dérivées** : MaxHp, QAm (attaque puissante), QAc (attaque critique), QDp (parade), QDe (esquive)
- **`CharacterBuilder`** — accumule les choix, applique les modificateurs (avec variance aléatoire)
- **`Injury`** — blessures typées : Physical, Mental, Energy avec Severity
- **`CharacterRules`** — chargé depuis `rules/character_rules.json`

---

## Système d'events (DungeonCrawler.EventSystems)

- **`IEventScript`** — use case C# avec paramètres typés configurables depuis le toolset
- **`EventScriptContext`** — API complète : WorldState, actions immédiates, actions différées
- **`EventScriptRegistry`** — scripts built-in + custom campagne
- **`EventSystem`** — triggers : GameStart, MapEnter, TileEnter, TurnPassed, Interact, Proximity
- **`IGameAction`** — actions différées traitées par PlayingScreen (StartDialogue, GiveItem, etc.)

---

## Système d'écrans (DungeonCrawler.Raylib)

Pattern `IGameScreen` : chaque écran implémente `OnEnter`, `Update`, `Draw`, `OnExit`.  
`Update()` retourne le prochain écran (ou `null` pour rester).  
`GameScreenRunner` gère la fenêtre Raylib et les transitions.

```
MainMenuScreen
  └── SlotSelectScreen
        ├── CharacterCreationScreen (6 étapes) → PlayingScreen
        └── (chargement save)                  → PlayingScreen
                                                      ↕ I
                                                  StatsScreen
```

`PlayingScreen` : `F5` = quicksave, `I` = stats, dialogue overlay bloque les inputs.

---

## Éditeur de maps (MapEditor.Avalonia)

- Ouvre un **projet campagne** (`.campaign.json`)
- Palette de tiles et d'entités par biome (module)
- Canvas interactif : peindre, effacer, sélectionner
- Transitions entre maps avec création de la transition retour automatique
- Navigateur de maps, projets récents
- Menu **Personnages → Règles de création** : éditeur de backgrounds, types et skills

---

## Moteur de dialogue (DialogueEngine.Core)

Moteur générique découplé de tout moteur de jeu.
Utilisé dans le jeu via `DialogueOverlay` (overlay typewriter sur PlayingScreen).

---

## Campagne Nostro

Dark fantasy dungeon crawler style Might & Magic 3.  
Mouvement case par case, tour par tour, vue première personne.

**Assets** :
- Police : MedievalSharp
- Palette : brun cuir / or fané / noir profond / beige parchemin
- Textures : pierre, sol, plafond, portes

**Config** : `src/Nostro/NostroConfig.cs`  
**Règles personnages** : `src/Nostro/rules/character_rules.json`  
**Dialogues** : `src/Nostro/dialogues/`