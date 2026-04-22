# Changelog

Toutes les versions notables de ce projet sont documentées ici.  
Format basé sur [Keep a Changelog](https://keepachangelog.com/fr/1.0.0/).

---

## [Unreleased]

### Prévu
- Édition des variantes de texte localisé dans l'éditeur
- Validation au chargement du `DialogueRunner`

---

## [0.1.0] — 2025-01-xx

### Ajouté
- `DialogueEngine.Core` — moteur de dialogue complet
  - `DialogueRunner` avec résolution conditionnelle des nœuds et réponses
  - `nextNodeIds[]` — liste ordonnée de candidats avec fallback
  - `ScriptRegistry` — conditions et conséquences par clé
  - `IDialogueContext` + `IVariableResolver` — substitution `{variable}`
  - `LocalizedText` — variantes conditionnelles (genre, race…)
  - Events `OnNodeEntered`, `OnDialogueEnd`, `OnDialogueCancelled`
  - `CancelConsequenceKey` par nœud
- `DialogueEngine.Serialization` — sérialisation JSON (camelCase)
  - `LocalizedTextConverter` — format union string | TextVariant[]
  - `DialogueFileSerializer` — sync + async
- 10 tests unitaires

[Unreleased]: https://github.com/TON_USERNAME/DialogueEngine/compare/v0.1.0...HEAD
[0.1.0]: https://github.com/TON_USERNAME/DialogueEngine/releases/tag/v0.1.0
