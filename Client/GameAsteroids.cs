using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Graphics;
using Monogame.Processing;
using System.Text.Json;
using System.Collections.Generic;
using Client.Entities;
using Client.Screens;
using Client.Rede;

public class GameAsteroids : Processing
{
    public SpriteBatch spriteBatch;
    public SpriteFont gameFont;

    /* --------------------- estado de jogo --------------------- */
    private ScreenManager currentScreen;
    public JsonElement currentGameState;
    public int level = 0;

    /* --------------------- telas de jogo ---------------------- */
    private Dictionary<ScreenManager, IScreen> screens;

    /* --------------------- teclado --------------------------- */
    public InputState input = new InputState();

    /* --------------------- sprites --------------------------- */
    public PImage pterosaurSprite;
    public PImage asteroidSpriteSmall;
    public List<PImage> gameAsteroidsSprites;
    public List<PImage> screenAsteroidsSprites;

    private const int MAX_ASTEROIDS = 6;
    private const int NUM_ASTEROIDS_SPRITES = 4;

    /* --------------------- rede ------------------------------ */
    private Connection connection;

    /* --------------------- fábricas -------------------------- */
    private AsteroidFactory asteroidFactory;

    /* ===================== ciclo de vida ======================= */
    public override void Setup()
    {
        size(800, 600);

        spriteBatch = new SpriteBatch(GraphicsDevice);
        gameFont = Content.Load<SpriteFont>("PressStart");
        currentGameState = new JsonElement();

        LoadSprites();
        InitScreens();

        connection = Connection.GetInstance(this);
        asteroidFactory = new AsteroidFactory(this);

        currentScreen = ScreenManager.Menu;
    }

    private void LoadSprites()
    {
        pterosaurSprite = loadImage("./Content/Sprites/pterosaur.png");
        asteroidSpriteSmall = loadImage("./Content/Sprites/asteroid_small.png");

        gameAsteroidsSprites = new List<PImage>();
        screenAsteroidsSprites = new List<PImage>();
        for (int i = 0; i < NUM_ASTEROIDS_SPRITES; i++)
        {
            gameAsteroidsSprites.Add(loadImage($"./Content/Sprites/GameAsteroids/asteroid{i + 1}.png"));
            screenAsteroidsSprites.Add(loadImage($"./Content/Sprites/ScreenAsteroids/asteroid{i + 1}.png"));
        }
    }

    private void InitScreens()
    {
        screens = new Dictionary<ScreenManager, IScreen>()
        {
            { ScreenManager.Menu, new MenuScreen(this) },
            { ScreenManager.StoryMode, new StoryScreen(this) },
            { ScreenManager.Playing, new GameScreen(this) },
            { ScreenManager.OnlinePlaying, new GameScreenOnline(this) },
            { ScreenManager.PauseScreen, new PauseScreen(this) },
            { ScreenManager.GameOver, new GameOverScreen(this) },
            { ScreenManager.Connection, new ConnectionScreen(this) },
            { ScreenManager.Disconnection, new DisconnectionScreen(this) }
        };

        foreach (var screen in screens.Values)
            screen.LoadContent();
    }

    /* ====================== loop principal ==================== */
    public void Update() => screens[currentScreen].Update();

    public override void Draw()
    {
        background(0);
        screens[currentScreen].Draw(
            currentScreen == ScreenManager.OnlinePlaying ? currentGameState : null
        );
        Update();
    }

    /* ====================== gestão de telas ================== */
    public void setCurrentScreen(ScreenManager newScreen, bool restart = false)
    {
        if ((newScreen == ScreenManager.Playing || newScreen == ScreenManager.Menu) && restart)
        {
            ResetScreens();
        }

        if ((currentScreen == ScreenManager.Playing || currentScreen == ScreenManager.Connection) &&
            (newScreen == ScreenManager.Menu || newScreen == ScreenManager.Disconnection))
        {
            connection.DisconnectFromServer();
        }

        currentScreen = newScreen;
        delay(150);
    }

    private void ResetScreens()
    {
        foreach (var screen in screens.Values)
            screen.Reset();
    }

    public ScreenManager getCurrentScreen() => currentScreen;

    /* ====================== Input ============================ */
    public override void KeyPressed(Keys key) => input.KeyPressed(key);
    public override void KeyReleased(Keys key) => input.KeyReleased(key);

    /* ====================== utilitários ====================== */
    public void DrawText(string text, SpriteFont font, Vector2 position, Color color, float scale)
    {
        spriteBatch.Begin();
        Vector2 textSize = font.MeasureString(text);
        spriteBatch.DrawString(font, text, position, color, 0f, textSize / 2f, scale, SpriteEffects.None, 0f);
        spriteBatch.End();
    }

    /* ====================== fábrica de asteroids ============= */
    public Asteroid NovoAsteroid(bool isGame) => asteroidFactory.CreateAsteroid(isGame);

    public void DrawAsteroidsBackground(List<Asteroid> asteroids) { 
        if (frameCount % 150 == 0 && asteroids.Count < MAX_ASTEROIDS) 
        asteroids.Add(NovoAsteroid(false)); 
        for (int i = asteroids.Count - 1; i >= 0; i--) 
        {
            var a = asteroids[i]; 
            a.Update(); 
            a.Draw(this); 
            if (a.getPosition().Y > height + 50 || a.getPosition().X < -50 || a.getPosition().X > width + 50) 
            { 
                asteroids.RemoveAt(i); 
                asteroids.Add(NovoAsteroid(false)); 
            } 
        } 
    }

    /* ====================== entry-point ====================== */
    [STAThread]
    static void Main()
    {
        using var jogo = new GameAsteroids();
        jogo.Run();
    }
}
