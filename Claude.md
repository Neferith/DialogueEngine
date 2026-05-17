# Notes techniques pour Claude

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