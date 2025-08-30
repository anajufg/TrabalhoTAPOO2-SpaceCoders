using Monogame.Processing;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Client.Entities;

namespace Cliente.Screens;

public class MenuScreen
{
    private GameAsteroids p;
    private PImage backgroundImage;
    private PImage asteroidSprite;
    private SpriteFont font;
    private SpriteBatch spriteBatch;

    private enum MenuOption { Play, StoryMode, Exit };
    private readonly MenuOption[] menuOptions = { MenuOption.Play, MenuOption.StoryMode, MenuOption.Exit };
    private int selectedIndex = 0;
    private bool wasKeyPressedLastFrame = false;

    private const int MAX_ASTEROIDS = 6;
    private readonly List<Asteroid> asteroids = new();

    public MenuScreen(GameAsteroids p)
    {
        this.p = p;
    }

    public void LoadContent()
    {
        spriteBatch = new SpriteBatch(p.GraphicsDevice);
        font = p.Content.Load<SpriteFont>("PressStart"); 
        backgroundImage = p.loadImage("./Content/Backgrounds/menu_background.png");
        asteroidSprite = p.loadImage("./Content/Sprites/asteroid.png");
    }

    public void Draw()
    {
        p.image(backgroundImage, 0, 0, p.width, p.height);

        Asteroids();

        spriteBatch.Begin();

        string title = "Jurassic Impact";
        Vector2 titleSize = font.MeasureString(title);
        Vector2 titlePos = new(p.width / 2f, p.height * 0.4f); 
        spriteBatch.DrawString(font, title, titlePos, Color.Yellow,
            0f, titleSize / 2f, 1f, SpriteEffects.None, 0f);

        string subtitle = "Prepare-se para mudar o passado!";
        Vector2 subtitleSize = font.MeasureString(subtitle);
        Vector2 subtitlePos = new(p.width / 2f, titlePos.Y + titleSize.Y + 20f); 
        spriteBatch.DrawString(font, subtitle, subtitlePos, Color.LightGray,
            0f, subtitleSize / 2f, 0.7f, SpriteEffects.None, 0f); 

        Vector2 basePos = new(p.width / 2f, p.height - 180f);
        float lineSpacing = 30f; // Espaçamento entre as opções

        for (int i = 0; i < menuOptions.Length; i++)
        {
            string text = menuOptions[i].ToString();
            Vector2 textSize = font.MeasureString(text);
            Vector2 textPos = new(basePos.X, basePos.Y + i * lineSpacing);
            
            Color textColor = (i == selectedIndex && (p.frameCount / 30) % 2 == 0) ? Color.Transparent : Color.White;
            
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
                    selectedIndex = (selectedIndex - 1 + menuOptions.Length) % menuOptions.Length;
                    break;
                case Keys.Down: 
                    selectedIndex = (selectedIndex + 1) % menuOptions.Length;
                    break;
                case Keys.Space:
                    MenuOption currentOption = menuOptions[selectedIndex];
                    
                    if (currentOption == MenuOption.Play)
                    {
                        p.setCurrentScreen(ScreenManager.Playing);
                    }
                    else if (currentOption == MenuOption.StoryMode)
                    {
                        p.setCurrentScreen(ScreenManager.StoryMode);
                    }
                    else if (currentOption == MenuOption.Exit)
                    {
                        p.Exit();
                    }
                    break;
            }
        }
        
        wasKeyPressedLastFrame = isKeyPressedNow;
    }

    private Asteroid NovoAsteroid()
    {
        Random rnd = new Random();
        float x = p.width / 2 + rnd.Next(50, 400);
        float y = -30 - rnd.Next(0, 50);

        float targetX = -350;
        float targetY = p.height + 30;

        Vector2 dir = new Vector2(targetX - x, targetY - y);
        dir.Normalize();

        float speed = 0.8f + (float)rnd.NextDouble() * 0.5f;
        float size = 15f + (float)rnd.NextDouble() * 70f;

        return new Asteroid(new Vector2(x, y), dir * speed, size, asteroidSprite);
    }

    private void Asteroids() 
    {
        if (p.frameCount % 150 == 0 && asteroids.Count < MAX_ASTEROIDS)
            asteroids.Add(NovoAsteroid());

        for (int i = asteroids.Count - 1; i >= 0; i--)
        {
            var a = asteroids[i];
            a.Update();
            a.Draw(p);

            if (a.getPosition().Y > p.height + 50 ||
                a.getPosition().X < -50 ||
                a.getPosition().X > p.width + 50)
            {
                asteroids.RemoveAt(i);
                asteroids.Add(NovoAsteroid());
            }
        }
    }
}
