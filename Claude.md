# Notes techniques pour Claude

## Projet

Solution C# .NET 8 combinant un moteur de dungeon crawler, un éditeur de maps,
un moteur de dialogue, et une campagne jouable (Nostro).
Le nom de la solution est temporairement "DialogueEngine2" — renommage prévu.

---

## Coordination des axes entre éditeur et moteur

**Problème** : L'éditeur (`MapEditor.Avalonia`) utilise des coordonnées écran (Y=0 en haut,
Y croissant vers le bas). Le moteur (`DungeonCrawler.Core`) utilise des coordonnées
mathématiques (Y=0 en bas, Y croissant vers le haut).

**Symptôme** : Sans correction, gauche et droite sont inversés dans le jeu quand on
charge une map créée dans l'éditeur.

**Fix** : Dans `MapFileLoader.BuildLoadedMap()`, flipper Y à chaque lecture de position
issue du `MapFile` :

```csharp
int FlipY(int y) => height - 1 - y;
```

À appliquer sur :
- Les positions des tiles overrides
- Les positions des transitions
- Les positions des entités (PlayerSpawn et futures entités)

**Ne pas** flipper Y dans les coordonnées cibles des transitions (TargetPosition) —
elles seront flippées lors du chargement de la map cible.

---

## Transitions entre maps

**Convention** : la transition est posée sur la tile PORTE. `TargetPosition` = case
d'arrivée (walkable, devant la porte côté cible).

**Formule position de la porte côté cible** (coords éditeur) :
```
doorPos = arrivalPos + offset(arrivalOrientation)
```
— **pas minus** — le FlipY inverse la relation, donc on additionne.

**Formule spawn retour** :
```
returnSpawn = sourceDoor + offset(returnOrientation)
```

---

## Portes de transition

- La porte s'ouvre via `HandleInteraction()` → `IsSolid = false`, `Tag = DoorOpen`
- Elle **ne disparaît pas** — `TileTag.DoorOpen` existe pour ça
- Le rendu DungeonRenderer gère `DoorOpen` : affiche la texture porte ouverte + sol/plafond

---

## Architecture générale

```
DialogueEngine.Core          ← moteur dialogue pur
DialogueEngine.Serialization ← JSON dialogues
DialogueEngine.Editor        ← éditeur Avalonia

DungeonCrawler.Core          ← moteur donjon pur
  └── Core.Persist           ← DTOs de sauvegarde (SaveFile, CharacterSaveData)
DungeonCrawler.Characters    ← système RPG personnages (indépendant de Core)
DungeonCrawler.Raylib        ← renderer + IGameScreen + screens + mappers
DungeonCrawler.MapLoader     ← pont éditeur ↔ moteur

MapEditor.Core               ← modèles maps + modules + CharacterRulesFile
MapEditor.Avalonia           ← éditeur visuel + éditeur CharacterRules

Nostro                       ← campagne jouable (Exe)
  └── rules/character_rules.json
```

**Règle de dépendance** :
- `DungeonCrawler.Core` ne connaît ni Raylib ni MapEditor ni Characters
- `DungeonCrawler.Characters` est totalement indépendant
- `DungeonCrawler.Raylib` référence Core + Characters + MapLoader
- `DungeonCrawler.MapLoader` fait le pont (Core + MapEditor.Core)
- `Nostro` est l'Exe qui assemble tout

---

## Système d'écrans (DungeonCrawler.Raylib)

- `IGameScreen` : `OnEnter()`, `Update(float dt) → IGameScreen?`, `Draw(int w, int h)`, `OnExit()`
- `GameScreenRunner` : gère fenêtre Raylib + transitions
- **`Raylib.SetExitKey(KeyboardKey.Null)`** dans `Run()` — obligatoire sinon Escape ferme la fenêtre
- Flow : `MainMenuScreen` → `SlotSelectScreen` → `CharacterCreationScreen` → `PlayingScreen`
- `PlayingScreen` → `I` → `StatsScreen` → `Escape` → `PlayingScreen`
- `PlayingScreen` : `F5` = quicksave, `I` = stats

---

## Système de sauvegarde

- `SaveFile` + `CharacterSaveData` + `InjurySaveData` dans `DungeonCrawler.Core.Persist`
- `SaveManager` : `%AppData%/{campaignName}/saves/slot_N.json`, 5 slots
- `CharacterMapper` dans `DungeonCrawler.Raylib` : `Character` ↔ `CharacterSaveData`
- `ActiveSave(SaveManager, SlotIndex, HeroName, List<Character>)` : contexte en cours de partie
- Coordonnées sauvegardées = coords JEU (déjà flippées)
- `DungeonCrawler.Core.Persist` → futur projet `DungeonCrawler.Persistence` (dette tech)

---

## Système de personnages (DungeonCrawler.Characters)

**Chaîne de création** : `Genre → Taille → Poids → Sensibilité → Background`
- Chaque étape filtre les options suivantes
- `ApplyModifier` = aléatoire borné (ex : +3 → donne 0, 1, 2 ou 3)
- `ApplyFixed` = déterministe

**Stats dérivées** :
- `MaxHp = Vitality×2 + Musculature + 15`
- `QAm (Attaque Puissante) = Musculature×2 + Vitality`
- `QAc (Attaque Critique)  = Brain×2 + Flexibility`
- `QDp (Défense Parade)    = Vitality×2 + Musculature`
- `QDe (Défense Esquive)   = Flexibility×2 + Brain`

**CharacterBuilder** : accumule les choix, calcule les attributs, construit le `Character`.
Guard : si `_backgroundTypes.Count == 0`, `IsComplete` doit retourner `true` quand même.

**CharacterRules** : chargé depuis `config.CharacterRulesPath`.
Le fichier doit être copié dans l'output via `CopyToOutputDirectory` dans le `.csproj`.

---

## Éditeur MapEditor

- Ouvre un projet `.campaign.json`
- Menu **Personnages** → `CharacterRulesWindow` (popup séparée)
- `CharacterRulesPath` dans `CampaignProject` → `AbsoluteCharacterRulesPath` avec `[JsonIgnore]`
- `NumericUpDown.Value` est `decimal?` en Avalonia → propriétés VM en `decimal`, cast `(int)` dans `ToData()`
- Auto-création du `character_rules.json` si absent (dans `CharacterRulesViewModel.Load()`)

---

## Coordonnées : résumé

| Contexte       | Origine     | Y positif    |
|----------------|-------------|--------------|
| Éditeur canvas | haut-gauche | vers le bas  |
| JSON map       | haut-gauche | vers le bas  |
| Moteur jeu     | bas-gauche  | vers le haut |
| FlipY          | `gameY = height - 1 - editorY` | — |

---

## Pièges connus

- `UpdateTileData` avec `transition = null` **efface** une transition existante — vérifier `HasTransition` avant.
- `AvailableMaps` (ComboBox) peut se vider lors d'un `Initialize()` → toujours re-sélectionner après rebuild.
- `DrawRectangleRoundedLines` Raylib : signature `(Rectangle, roundness, segments, lineThick, Color)`.
- `RaylibColorScheme` doit être un `record` pour supporter `with { }`.
- `MapFileLoader` maintient un cache de modules — passer la **même instance** au `DungeonSession`.
- `Raylib.SetExitKey(KeyboardKey.Null)` obligatoire sinon Escape ferme la fenêtre.
- `CharacterAttribute` (pas `Attribute`) — conflit avec `System.Attribute`.
- `CharacterAttributes` (pas `Attributes`) — renommé pour cohérence.
- `BackgroundRequirementRaw` : `this((List<string>?)null)` pour lever l'ambiguïté du constructeur de copie.
- `FantasyUI.Init` : charger la police à 256px pour éviter la pixelisation.