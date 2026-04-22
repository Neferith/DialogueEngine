# DialogueEngine

Moteur de dialogue générique en C# .NET 8, découplé de tout moteur de jeu.  
Conçu pour être embarqué dans un projet Godot, Avalonia, ou tout autre hôte .NET.

---

## Structure de la solution

```
DialogueEngine/
├── src/
│   ├── DialogueEngine.Core/          Moteur pur — aucune dépendance UI
│   ├── DialogueEngine.Serialization/ Lecture/écriture JSON des dialogues
│   ├── DialogueEngine.Editor/        Éditeur de dialogues (Avalonia 11)
│   ├── Sample1/                      Démo — conversation NES-style (statique)
│   └── Sample2/                      Démo — jeu vue de dessus avec infiltration
└── tests/
    └── DialogueEngine.Core.Tests/    Tests unitaires (xUnit + FluentAssertions)
```

---

## Prérequis

- .NET 8 SDK
- Visual Studio 2022 17.8+ ou Rider 2023.3+

---

## Lancer les projets

```bash
# Éditeur de dialogues
dotnet run --project src/DialogueEngine.Editor

# Sample 1 — conversation avec un garde
dotnet run --project src/Sample1

# Sample 2 — infiltration vue de dessus
dotnet run --project src/Sample2

# Tests
dotnet test
```

---

## Architecture du moteur

### Modèle de données

```
DialogueFile
└── nodes: Node[]          ← liste ordonnée

Node
├── id: string
├── conditionKey: string?  ← script → bool, null = toujours vrai
├── text: LocalizedText    ← string simple OU variantes conditionnelles
├── consequenceKey: string?
├── cancelConsequenceKey: string?
└── responses: Response[]

Response
├── text: LocalizedText
├── conditionKey: string?
├── consequenceKey: string?
└── nextNodeIds: string[]  ← liste ordonnée de candidats
```

### Résolution au runtime

Le moteur parcourt `nodes[]` dans l'ordre et affiche **le premier dont la condition passe**.  
Si aucun nœud ne passe, le dialogue se termine silencieusement.

Pour `nextNodeIds[]`, même mécanique : le moteur prend le premier nœud cible dont la condition passe.  
Vide = fin du dialogue.

### Cycle de vie

```
Start(file, context)
  → OnNodeEntered(ResolvedNode)   ← texte + réponses prêts à l'affichage

Select(index)
  → exécute la conséquence de la réponse
  → résout le prochain nœud
  → OnNodeEntered  OU  OnDialogueEnd

Cancel()
  → exécute CancelConsequenceKey du nœud courant
  → OnDialogueCancelled(nodeId)
```

---

## Format JSON d'un dialogue

```json
{
  "id": "mon_dialogue",
  "nodes": [
    {
      "id": "intro",
      "conditionKey": "player_is_officer",
      "text": "Bienvenue, {player.name}.",
      "cancelConsequenceKey": "npc_annoyed",
      "responses": [
        {
          "text": "[Charme] Vous semblez fatigué...",
          "conditionKey": "has_charm",
          "consequenceKey": "set_npc_charmed",
          "nextNodeIds": ["npc_gives_pass"]
        },
        {
          "text": "Je passe mon chemin.",
          "nextNodeIds": []
        }
      ]
    },
    {
      "id": "npc_gives_pass",
      "text": "Tenez, voilà votre laissez-passer.",
      "responses": [
        { "text": "Merci.", "nextNodeIds": [] }
      ]
    }
  ]
}
```

### Texte localisé (variantes conditionnelles)

```json
"text": [
  { "conditionKey": "player_female", "value": "Bienvenue, commandante {player.name}." },
  { "value": "Bienvenue, commandant {player.name}." }
]
```

La dernière variante sans `conditionKey` est le **fallback obligatoire**.

---

## Intégration dans un projet

### 1. Enregistrer les scripts

```csharp
var registry = new ScriptRegistry()
    .Condition("player_is_officer", ctx => gameState.Rank == Rank.Officer)
    .Condition("has_charm",         ctx => gameState.Skill == Skill.Charm)
    .Consequence("set_npc_charmed", ctx => gameState.NpcEmotion = Emotion.Charmed)
    .Consequence("unlock_door",     ctx => gameState.DoorOpen = true);
```

### 2. Implémenter le contexte

```csharp
public class MyContext : IDialogueContext
{
    public IVariableResolver Variables { get; }

    public MyContext(GameState state)
    {
        Variables = new LambdaResolver(key => key switch
        {
            "player.name" => state.PlayerName,
            _             => $"{{{key}}}"
        });
    }
}
```

### 3. Démarrer un dialogue

```csharp
var runner = new DialogueRunner(registry);

runner.OnNodeEntered      += node => ui.Show(node.Text, node.Responses);
runner.OnDialogueEnd      += ()   => ui.HideDialogue();
runner.OnDialogueCancelled += id  => ui.HideDialogue();

var file = DialogueFileSerializer.Deserialize(json);
runner.Start(file, new MyContext(gameState));

// Quand le joueur choisit une réponse :
runner.Select(responseIndex);
```

---

## Éditeur de dialogues

Interface Avalonia permettant de créer et éditer des fichiers `.json` de dialogue.

- Panel gauche : liste et ordre des nœuds
- Panel droit : édition du nœud sélectionné (texte, conditions, conséquences, réponses)
- Validation structurelle avant sauvegarde (références de nœuds, fallbacks)
- `nextNodeIds` édités comme liste texte (un ID par ligne, ordre = priorité)

---

## Tests

```
DialogueEngine.Core.Tests
├── Starts on first node
├── Skips node when condition false
├── Ends when no node passes
├── Selects response and navigates
├── Response with multiple nextNodeIds picks first passing
├── Empty nextNodeIds ends dialogue
├── Filters responses by condition
├── Substitutes variables in text
├── Cancel fires event with nodeId and executes consequence
├── Cannot start while active
└── Serialization roundtrip
```
