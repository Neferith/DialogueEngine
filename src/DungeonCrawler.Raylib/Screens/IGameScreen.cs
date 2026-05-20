namespace DungeonCrawler.RaylibGame;

public interface IGameScreen
{
    /// <summary>Appelé une fois quand l'écran devient actif.</summary>
    void OnEnter();

    /// <summary>
    /// Mise à jour logique.
    /// Retourne un nouvel écran pour transitionner, null pour rester.
    /// </summary>
    IGameScreen? Update(float dt);

    /// <summary>Rendu. Appelé entre BeginDrawing / EndDrawing.</summary>
    void Draw(int screenWidth, int screenHeight);

    /// <summary>Appelé une fois quand l'écran est quitté.</summary>
    void OnExit();
}