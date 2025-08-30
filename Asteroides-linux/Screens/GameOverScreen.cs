using Monogame.Processing;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Client.Entities;

namespace Cliente.Screens;

public class GameOverScreen
{
    private GameAsteroids p;
    private PImage backgroundImage;
    private SpriteFont font;
    private SpriteBatch spriteBatch;

    private enum GameOverOptions { Restart, Menu, Exit };
    private readonly GameOverOptions[] GameOverOptionss;
    private int selectedIndex;
    private bool wasKeyPressedLastFrame;

    private int score;

    private List<Asteroid> asteroids;

    public GameOverScreen(GameAsteroids p)
    {
        this.p = p;
        asteroids = new();
        GameOverOptionss = [GameOverOptions.Restart, GameOverOptions.Menu, GameOverOptions.Exit];
        selectedIndex = 0;
        wasKeyPressedLastFrame = false;
        score = 0;
    }

    public void LoadContent()
    {
        spriteBatch = new SpriteBatch(p.GraphicsDevice);
        font = p.Content.Load<SpriteFont>("PressStart"); 
        backgroundImage = p.loadImage("./Content/Backgrounds/gameOver_background.png");
    }

    public void Draw()
    {
        p.image(backgroundImage, 0, 0, p.width, p.height);

        p.DrawAsteroidsBackground(asteroids);

        p.DrawText("Game Over", font, new Vector2(p.width / 2f, p.height * 0.2f), Color.Yellow, 1.5f);
        p.DrawText($"Score: {score}", font, new Vector2(p.width / 2f, p.height * 0.30f), Color.White, 0.8f);
        
        Vector2 basePos = new(p.width / 2f, p.height * 0.5f);
        float lineSpacing = 30f; 

        for (int i = 0; i < GameOverOptionss.Length; i++)
        {
            string text = GameOverOptionss[i].ToString();
            Vector2 textPos = new(basePos.X, basePos.Y + i * lineSpacing);
            Color textColor = (i == selectedIndex && (p.frameCount / 30) % 2 == 0) ? Color.Transparent : Color.LightGray;
            
            p.DrawText(text, font, textPos, textColor, 0.5f);
        }
        
    }

    public void Update()
    {
        bool isKeyPressedNow = p.keyPressed;

        if (isKeyPressedNow && !wasKeyPressedLastFrame)
        {
            switch (p.keyCode)
            {
                case Keys.Up: 
                    selectedIndex = (selectedIndex - 1 + GameOverOptionss.Length) % GameOverOptionss.Length;
                    break;
                case Keys.Down: 
                    selectedIndex = (selectedIndex + 1) % GameOverOptionss.Length;
                    break;
                case Keys.Space:
                    GameOverOptions currentOption = GameOverOptionss[selectedIndex];
                    
                    switch(currentOption) 
                    {
                        case GameOverOptions.Restart:
                            p.setCurrentScreen(ScreenManager.Playing, true);
                            break;
                        case GameOverOptions.Menu:
                            p.setCurrentScreen(ScreenManager.Menu, true);
                            break;
                        case GameOverOptions.Exit:
                            p.Exit();
                            break;
                    }
                    break;
            }
        }
        
        wasKeyPressedLastFrame = isKeyPressedNow;
    }

    public void Reset()
    {
        asteroids = new();
        selectedIndex = 0;
        wasKeyPressedLastFrame = false;
        score = 0;
    }

    public void setScore(int score) { this.score = score; }
}
