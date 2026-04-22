# Agenda — DialogueEngine

---

## ✅ Fait

### Moteur (DialogueEngine.Core)
- [x] Modèle de données : `DialogueFile`, `Node`, `Response`, `LocalizedText`, `TextVariant`
- [x] `DialogueRunner` — résolution conditionnelle des nœuds et des réponses
- [x] `nextNodeIds[]` — liste ordonnée de candidats avec fallback conditionnel
- [x] `ScriptRegistry` — conditions et conséquences enregistrées par clé
- [x] `IDialogueContext` + `IVariableResolver` — substitution `{variable}` dans les textes
- [x] Texte localisé — variantes conditionnelles avec fallback obligatoire
- [x] Events : `OnNodeEntered`, `OnDialogueEnd`, `OnDialogueCancelled`
- [x] `CancelConsequenceKey` par nœud — réaction à l'annulation localisée

### Sérialisation (DialogueEngine.Serialization)
- [x] `DialogueFileSerializer` — lecture/écriture JSON (camelCase)
- [x] `LocalizedTextConverter` — format union string | TextVariant[]

### Tests (DialogueEngine.Core.Tests)
- [x] 10 tests couvrant les cas nominaux et limites du moteur
- [x] Roundtrip de sérialisation

### Éditeur (DialogueEngine.Editor)
- [x] Architecture MVVM propre — un ViewModel par vue
- [x] Pattern factory-container — `EditorContainer` implémente `INodeListFactory` + `INodeEditorFactory`
- [x] Ouverture / sauvegarde / enregistrer sous
- [x] Validation inline (IDs vides, références cassées)
- [x] Édition nœuds : id, condition, conséquence, cancel, texte, réponses
- [x] `nextNodeIds` édités comme liste texte (un ID par ligne)
- [x] Ordonnancement des nœuds et des réponses (▲▼)
- [x] Duplication de nœud

### Sample1
- [x] Conversation statique avec un garde
- [x] Sélection de rang au démarrage (Civil, Soldat, Officier, Déserteur)
- [x] Branches de dialogue différentes par rang (conditions sur les nœuds)
- [x] Effet typewriter + curseur clignotant
- [x] Écran de résultat selon les flags déclenchés

### Sample2
- [x] Vue de dessus — déplacement ZQSD / flèches
- [x] Mur de partition avec ouverture (porte franchissable)
- [x] Deux PNJ avec zones d'interaction
- [x] Sélection de compétence (Charme, Intimidation, Bluff) → débloque des réponses
- [x] Réaction émotionnelle de l'officier (Charmé, Apeuré, Convaincu)
- [x] Dialogue post-laissez-passer contextuel selon l'émotion
- [x] Garde conditionnel (ouvre la porte uniquement avec laissez-passer)
- [x] Écran de fin avec recap de la compétence utilisée

---

## 🔧 Corrections connues / dette technique

- [ ] **Éditeur** — texte localisé (variantes race/genre) non encore éditable dans l'UI
- [ ] **Éditeur** — pas de prévisualisation du graphe de dialogue
- [ ] **Sample2** — pas de sprite animé pour le joueur (direction non indiquée visuellement)
- [ ] **Sample2** — le joueur peut se superposer visuellement aux PNJ (pas de collision NPC)
- [ ] **Moteur** — pas de validation au chargement (références cassées détectées seulement à l'exécution)
- [ ] **Moteur** — boucles de dialogue infinies possibles (détection optionnelle à ajouter)

---

## 📋 Court terme

### Jeu shoot em up RPG (objectif principal)
- [ ] Définir la structure des phases : création personnage → création vaisseau → pré-mission → mission
- [ ] Implémenter la phase de création de personnage
- [ ] Implémenter la phase de création de vaisseau
- [ ] Hub inter-mission : carte de la base avec PNJ et lieux navigables
- [ ] Intégrer le moteur de dialogue dans le hub
- [ ] `CampaignState` — flags persistants entre les phases

### Moteur de dialogue
- [ ] Validation au chargement (option `DialogueValidator`)
- [ ] Support des boucles de dialogue délibérées (back-links documentés dans l'éditeur)
- [ ] Historique de conversation accessible au runtime

### Éditeur
- [ ] Édition des variantes de texte localisé (mode simple ↔ variantes)
- [ ] Indicateur visuel des nœuds avec références cassées
- [ ] Raccourcis clavier (Ctrl+S, Ctrl+N, Ctrl+O)

---

## 🎯 Vision long terme

### Shoot em up RPG
- [ ] Phase mission : shoot em up horizontal (Godot 4 ou Avalonia Canvas)
- [ ] Système d'armes et compétences vaisseau
- [ ] Missions linéaires avec branchements selon les choix pré-mission
- [ ] Debriefing avec récompenses et progression
- [ ] Sauvegarde de campagne

### Moteur de dialogue
- [ ] Export vers format intermédiaire (pour Godot GDScript, Unity C#)
- [ ] Support du scripting embarqué (expressions simples sans enregistrement externe)
- [ ] Localisation multi-langues (i18n via clés de traduction)
- [ ] Arbre de dialogue visuel dans l'éditeur (node editor style)
- [ ] Système de portait/avatar de locuteur

### Qualité
- [ ] Tests d'intégration Sample1 et Sample2
- [ ] Benchmark du moteur sur de grands graphes (1000+ nœuds)
- [ ] Documentation XML complète sur toute l'API publique

---

## 📝 Notes d'architecture

**Règle de dépendance** : `Core` ← `Serialization` ← `Editor` / `Sample*`  
Le `Core` n'a aucune dépendance externe (pas d'Avalonia, pas de Godot).

**Pattern DI** : factory-container. Chaque classe déclare l'interface factory dont elle a besoin.  
Le container du shell (ex : `EditorContainer`) les implémente toutes et passe `this` là où c'est nécessaire.

**Scripts de condition/conséquence** : clés string enregistrées dans `ScriptRegistry`.  
Le jeu fournit les lambdas, le moteur les appelle. Aucun couplage direct.
