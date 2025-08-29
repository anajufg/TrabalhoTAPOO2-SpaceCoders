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

    private enum MenuOption { Play, StoryMode };
    private MenuOption selectedOption = MenuOption.Play;

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

        string play = "Play";
        Vector2 playSize = font.MeasureString(play);
        Vector2 playPos = new(p.width / 2f, p.height - 250f); 
        Color playColor = (selectedOption == MenuOption.Play && (p.frameCount / 30) % 2 == 0) ? Color.Transparent : Color.White;
        spriteBatch.DrawString(font, play, playPos,  playColor,
            0f, playSize / 2f, 0.5f, SpriteEffects.None, 0f);

        string storyMode = "Story Mode";
        Vector2 storyModeSize = font.MeasureString(storyMode);
        Vector2 storyModePos = new(p.width / 2f, p.height - 225f); 
        Color storyModeColor = (selectedOption == MenuOption.StoryMode && (p.frameCount / 30) % 2 == 0) ? Color.Transparent : Color.White;
        spriteBatch.DrawString(font, storyMode, storyModePos, storyModeColor,
        0f, storyModeSize / 2f, 0.5f, SpriteEffects.None, 0f);
    
        spriteBatch.End();
    }

    public void Update()
    {
        if (p.keyPressed)
        {
            switch (p.keyCode)
            {
                case Keys.Up: 
                    if (selectedOption == MenuOption.StoryMode) selectedOption = MenuOption.Play;
                    break;
                case Keys.Down: 
                    if (selectedOption == MenuOption.Play) selectedOption = MenuOption.StoryMode;
                    break;
            }
        }

        if (p.keyPressed && p.keyCode == Keys.Space)
        {
            if (selectedOption == MenuOption.Play)
            {
                p.setCurrentScreen(ScreenManager.Playing);
            }
            else if (selectedOption == MenuOption.StoryMode)
            {
                p.setCurrentScreen(ScreenManager.StoryMode); 
            }
        }
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
