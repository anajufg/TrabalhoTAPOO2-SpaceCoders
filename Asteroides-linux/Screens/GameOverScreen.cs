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

    private readonly List<Asteroid> asteroids;

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

        spriteBatch.Begin();

        string gameOver = "Game Over";
        Vector2 gameOverSize = font.MeasureString(gameOver);
        Vector2 gameOverPos = new(p.width / 2f, p.height * 0.2f); 
        spriteBatch.DrawString(font, gameOver, gameOverPos, Color.Yellow,
            0f, gameOverSize / 2f, 1.5f, SpriteEffects.None, 0f);

        string scoreText = $"Score: {score}";
        Vector2 scoreSize = font.MeasureString(scoreText);
        Vector2 scorePos = new(p.width / 2f, gameOverPos.Y + gameOverSize.Y + 30f); 
        spriteBatch.DrawString(font, scoreText, scorePos, Color.White,
            0f, scoreSize / 2f, 0.8f, SpriteEffects.None, 0f); 

        Vector2 basePos = new(p.width / 2f, scorePos.Y + scoreSize.Y + 100f);
        float lineSpacing = 30f; 

        for (int i = 0; i < GameOverOptionss.Length; i++)
        {
            string text = GameOverOptionss[i].ToString();
            Vector2 textSize = font.MeasureString(text);
            Vector2 textPos = new(basePos.X, basePos.Y + i * lineSpacing);
            
            Color textColor = (i == selectedIndex && (p.frameCount / 30) % 2 == 0) ? Color.Transparent : Color.LightGray;
            
            spriteBatch.DrawString(font, text, textPos, textColor,
                0f, textSize / 2f, 0.5f, SpriteEffects.None, 0f);
        }
        
        spriteBatch.End();
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
                            p.setCurrentScreen(ScreenManager.Menu);
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

    public void setScore(int score) { this.score = score; }
}
