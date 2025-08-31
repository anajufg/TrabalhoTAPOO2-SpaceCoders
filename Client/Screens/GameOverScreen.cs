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

    private enum GameOverOption { Restart, Menu, Exit };
    private readonly GameOverOption[] gameOverOptions;
    private int selectedIndex;
    private bool wasKeyPressedLastFrame;

    private int score;

    private List<Asteroid> asteroids;

    public GameOverScreen(GameAsteroids p)
    {
        this.p = p;
        asteroids = new();
        gameOverOptions = [GameOverOption.Restart, GameOverOption.Menu, GameOverOption.Exit];
        selectedIndex = 0;
        wasKeyPressedLastFrame = false;
        score = 0;
    }

    public void LoadContent()
    { 
        backgroundImage = p.loadImage("./Content/Backgrounds/gameOver_background.png");
    }

    public void Draw()
    {
        p.image(backgroundImage, 0, 0, p.width, p.height);

        p.DrawAsteroidsBackground(asteroids);

        p.DrawText("Game Over", p.gameFont, new Vector2(p.width / 2f, p.height * 0.2f), Color.Red, 1.5f);
        p.DrawText($"Score: {score}", p.gameFont, new Vector2(p.width / 2f, p.height * 0.30f), Color.White, 0.8f);
        
        Vector2 basePos = new(p.width / 2f, p.height * 0.5f);
        float lineSpacing = 30f; 

        for (int i = 0; i < gameOverOptions.Length; i++)
        {
            string text = gameOverOptions[i].ToString();
            Vector2 textPos = new(basePos.X, basePos.Y + i * lineSpacing);
            Color textColor = (i == selectedIndex && (p.frameCount / 30) % 2 == 0) ? Color.Transparent : Color.LightGray;
            
            p.DrawText(text, p.gameFont, textPos, textColor, 0.5f);
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
                    selectedIndex = (selectedIndex - 1 + gameOverOptions.Length) % gameOverOptions.Length;
                    break;
                case Keys.Down: 
                    selectedIndex = (selectedIndex + 1) % gameOverOptions.Length;
                    break;
                case Keys.Space:
                    GameOverOption currentOption = gameOverOptions[selectedIndex];
                    
                    switch(currentOption) 
                    {
                        case GameOverOption.Restart:
                            p.setCurrentScreen(ScreenManager.Playing, true);
                            break;
                        case GameOverOption.Menu:
                            p.setCurrentScreen(ScreenManager.Menu, true);
                            break;
                        case GameOverOption.Exit:
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
