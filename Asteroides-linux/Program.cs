using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;   // só para comparar com Keys.*
using Monogame.Processing;
using Client.Entities;
using Cliente.Screens;

public class GameAsteroids : Processing
{
    /* --------------------- estado de jogo --------------------- */
    private ScreenManager currentScreen = ScreenManager.Menu;

    /* --------------------- telas de jogo --------------------- */
    private MenuScreen menuScreen;
    private StoryScreen storyScreen;
    private GameScreen gameScreen;
    private PauseScreen pauseScreen;

    /* --------------------- teclado (flags) -------------------- */
    public bool esquerda, direita, cima, baixo;

    /* ===================== ciclo de vida ====================== */
    public override void Setup()
    {
        size(800, 600);

        menuScreen = new MenuScreen(this);
        menuScreen.LoadContent();

        storyScreen = new StoryScreen(this);
        storyScreen.LoadContent();

        gameScreen = new GameScreen(this);
        gameScreen.LoadContent();

        pauseScreen = new PauseScreen(this);
        pauseScreen.LoadContent();
    }

    public void Update() 
    {
        switch (currentScreen)
        {
            case ScreenManager.Menu:
                menuScreen.Update();
                break;
            case ScreenManager.Playing:
                gameScreen.Update();
                break;
            case ScreenManager.StoryMode:
                storyScreen.Update(); 
                break;
            case ScreenManager.PauseScreen:
                pauseScreen.Update(); 
                break;
            case ScreenManager.GameOver:
                break;
        }
    }

    public override void Draw()
    {
        background(0);

        switch (currentScreen)
        {
            case ScreenManager.Menu:
                menuScreen.Draw();
                break;

            case ScreenManager.Playing:
                gameScreen.Draw();
                break;

            case ScreenManager.StoryMode:
                storyScreen.Draw();
                break;

            case ScreenManager.PauseScreen:
                gameScreen.Draw();
                pauseScreen.Draw();
                break;

            case ScreenManager.GameOver:
                fill(255, 0, 0);
                textSize(48);
                text("GAME OVER", width / 2f, height / 2f);
                /* ----- placar ----- */
                fill(255);
                textSize(20);
                // text($"Score: {score}", 10, 10);
                break;
        }

        Update();
    }

    public void setCurrentScreen(ScreenManager newScreen, bool restart = false) 
    {
        if (newScreen == ScreenManager.Playing && restart)
        {
            gameScreen = new GameScreen(this);
            gameScreen.LoadContent();
        }
        currentScreen = newScreen;
    }

    public override void KeyReleased(Keys pkey)
    {
        switch (char.ToUpperInvariant(key))
        {
            case 'A': esquerda = false; break;
            case 'D': direita = false; break;
            case 'W': cima = false; break;
            case 'S': baixo = false; break;
        }

        switch (keyCode)
        {
            case Keys.Left: esquerda = false; break;
            case Keys.Right: direita = false; break;
            case Keys.Up: cima = false; break;
            case Keys.Down: baixo = false; break;
        }
    }

    /* ====================== entry-point ======================= */
    [STAThread]
    static void Main()
    {
        using var jogo = new GameAsteroids();
        jogo.Run();
    }
}
