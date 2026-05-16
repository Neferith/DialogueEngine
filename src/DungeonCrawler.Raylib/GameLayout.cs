using Raylib_cs;

namespace DungeonCrawler.RaylibGame;

/// Décrit la disposition de la fenêtre à un instant donné.
public readonly record struct GameLayout(
    Rectangle ViewRect,   // vue 3D carrée
    Rectangle UiRect,     // panneau à droite
    Rectangle HudRect);   // barre en bas
