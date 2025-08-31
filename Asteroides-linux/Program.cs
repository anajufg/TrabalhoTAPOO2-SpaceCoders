using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;   // só para comparar com Keys.*
using Microsoft.Xna.Framework.Graphics;
using Monogame.Processing;
using Client.Entities;
using Cliente.Screens;

public class GameAsteroids : Processing
{
    private SpriteBatch spriteBatch;
    public SpriteFont gameFont;

    /* --------------------- estado de jogo --------------------- */
    private ScreenManager currentScreen;

    /* --------------------- telas de jogo ---------------------- */
    private MenuScreen menuScreen;
    private StoryScreen storyScreen;
    private GameScreen gameScreen;
    private PauseScreen pauseScreen;
    private GameOverScreen gameOverScreen;
    private DisconnectionScreen disconnectionScreen;
    private ConnectionScreen connectionScreen;

    /* --------------------- teclado (flags) --------------------- */
    public bool esquerda, direita, cima, baixo;

    /* --------------------- sprites ----------------------------- */
    private PImage asteroidSpriteSmall;
    private List<PImage> asteroidsSprites;

    private const int MAX_ASTEROIDS = 6;
    private const int NUM_ASTEROIDS_SPRITES = 4;

    /* ===================== ciclo de vida ======================= */
    public override void Setup()
    {
        size(800, 600);

        spriteBatch = new SpriteBatch(GraphicsDevice);
        gameFont = Content.Load<SpriteFont>("PressStart"); 

        asteroidsSprites = new();

        asteroidSpriteSmall = loadImage("./Content/Sprites/asteroid_small.png");
        for (int i = 0; i < NUM_ASTEROIDS_SPRITES; i++ )
        {
            asteroidsSprites.Add(loadImage($"./Content/Sprites/asteroid{i+1}.png"));
        }

        menuScreen = new MenuScreen(this);
        menuScreen.LoadContent();

        storyScreen = new StoryScreen(this);
        storyScreen.LoadContent();

        gameScreen = new GameScreen(this);
        gameScreen.LoadContent();

        pauseScreen = new PauseScreen(this);

        gameOverScreen = new GameOverScreen(this);
        gameOverScreen.LoadContent();

        disconnectionScreen = new DisconnectionScreen(this);
        disconnectionScreen.LoadContent();

        connectionScreen = new ConnectionScreen(this);
        connectionScreen.LoadContent();

        currentScreen = ScreenManager.Menu;
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
                gameOverScreen.Update();
                break;
            case ScreenManager.Connection:
                connectionScreen.Update();
                break;
            case ScreenManager.Disconnection:
                disconnectionScreen.Update();
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
                gameOverScreen.setScore(gameScreen.getScore());
                gameOverScreen.Draw();
                break;
            case ScreenManager.Connection:
                connectionScreen.Draw();
                break;
            case ScreenManager.Disconnection:
                disconnectionScreen.Draw();
                break;
        }

        Update();
    }

    /* ====================== fábrica de Asteroids =================== */
    public Asteroid NovoAsteroid(bool isGame)
    {
        Random rnd = new Random();
        PImage asteroidSprite;
        float x, y, speed, size;
        Vector2 dir;

        if (isGame)
        {
            int edge = rnd.Next(4);
            x = 0;
            y = 0;
            dir = Vector2.Zero;

            switch (edge)
            {
                case 0: // top
                    x = rnd.Next(width);
                    y = -30;
                    dir = new Vector2((float)(rnd.NextDouble() - 0.5), 1f);
                    break;
                case 1: // bottom
                    x = rnd.Next(width);
                    y = height + 30;
                    dir = new Vector2((float)(rnd.NextDouble() - 0.5), -1f);
                    break;
                case 2: // left
                    x = -30;
                    y = rnd.Next(height);
                    dir = new Vector2(1f, (float)(rnd.NextDouble() - 0.5));
                    break;
                case 3: // right
                    x = width + 30;
                    y = rnd.Next(height);
                    dir = new Vector2(-1f, (float)(rnd.NextDouble() - 0.5));
                    break;
            }

            speed = 2f + (float)rnd.NextDouble() * 2f; 
            size = 30f + (float)rnd.NextDouble() * 70f; 

            asteroidSprite = asteroidsSprites[rnd.Next(asteroidsSprites.Count)];
        }
        else 
        {
            x = width / 2 + rnd.Next(50, 400);
            y = -30 - rnd.Next(0, 50);

            float targetX = -350;
            float targetY = height + 30;

            dir = new Vector2(targetX - x, targetY - y);

            speed = 0.8f + (float)rnd.NextDouble() * 0.5f;
            size = 15f + (float)rnd.NextDouble() * 50f;
            
            if (size < 30f) asteroidSprite = asteroidSpriteSmall;
            else asteroidSprite = asteroidsSprites[rnd.Next(asteroidsSprites.Count)];
        }

        dir.Normalize();
        
        return new Asteroid(new Vector2(x, y), dir * speed, size, asteroidSprite);
    }

    public void DrawAsteroidsBackground(List<Asteroid> asteroids) 
    {
        if (frameCount % 150 == 0 && asteroids.Count < MAX_ASTEROIDS)
            asteroids.Add(NovoAsteroid(false));

        for (int i = asteroids.Count - 1; i >= 0; i--)
        {
            var a = asteroids[i];
            a.Update();
            a.Draw(this);

            if (a.getPosition().Y > height + 50 ||
                a.getPosition().X < -50 ||
                a.getPosition().X > width + 50)
            {
                asteroids.RemoveAt(i);
                asteroids.Add(NovoAsteroid(false));
            }
        }
    }

    private void Reset()
    {
        menuScreen.Reset();
        gameScreen.Reset();
        gameOverScreen.Reset();
        pauseScreen.Reset();
        storyScreen.Reset();
    }

    public void setCurrentScreen(ScreenManager newScreen, bool restart = false) 
    {
        if ((newScreen == ScreenManager.Playing || newScreen == ScreenManager.Menu) && restart)
        {
            Reset();
        } 
        currentScreen = newScreen;
        delay(150);
    }

    public void DrawText(string text, SpriteFont font, Vector2 position, Color color, float scale)
    {
        spriteBatch.Begin();
        Vector2 textSize = font.MeasureString(text);
        spriteBatch.DrawString(font, text, position, color,
            0f, textSize / 2f, scale, SpriteEffects.None, 0f);
        spriteBatch.End();
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

    /* ====================== entry-point ========================== */
    [STAThread]
    static void Main()
    {
        using var jogo = new GameAsteroids();
        jogo.Run();
    }
}