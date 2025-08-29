using Monogame.Processing;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Client.Entities;

namespace Cliente.Screens;

public class MenuScreen
{
    Processing p;
    PImage backgroundImage;
    PImage asteroidSprite;
    SpriteFont font;
    SpriteBatch spriteBatch;

    const int MAX_ASTEROIDS = 6;
    readonly List<Asteroid> asteroids = new();

    public MenuScreen(Processing p)
    {
        this.p = p;
    }

    public void LoadContent()
    {
        spriteBatch = new SpriteBatch(p.GraphicsDevice);

        font = p.Content.Load<SpriteFont>("Font"); 

        backgroundImage = p.loadImage("./Content/Backgrounds/menu_background.png");
        asteroidSprite = p.loadImage("./Content/Sprites/asteroid.png");
    }

    public void Draw()
    {
        // Fundo
        p.image(backgroundImage, 0, 0, p.width, p.height);

        // Asteroides
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

        // Texto com SpriteFont
        spriteBatch.Begin();

        // --- título ---
        string title = "ASTEROIDS";
        Vector2 titleSize = font.MeasureString(title);
        Vector2 titlePos = new(p.width / 2f, p.height / 3f);
        spriteBatch.DrawString(font, title, titlePos, Color.Yellow,
            0f, titleSize / 2f, 1f, SpriteEffects.None, 0f);

        // --- subtítulo ---
        string subtitle = "Prepare-se para a batalha espacial!";
        Vector2 subtitleSize = font.MeasureString(subtitle);
        Vector2 subtitlePos = new(p.width / 2f, p.height / 3f - 70f);
        spriteBatch.DrawString(font, subtitle, subtitlePos, Color.LightGray,
            0f, subtitleSize / 2f, 1f, SpriteEffects.None, 0f);

        // --- mensagem piscando ---
        if ((p.frameCount / 30) % 2 == 0)
        {
            string blink = "Pressione ENTER para jogar";
            Vector2 blinkSize = font.MeasureString(blink);
            Vector2 blinkPos = new(p.width / 2f, p.height - 100f);
            spriteBatch.DrawString(font, blink, blinkPos, Color.Yellow,
                0f, blinkSize / 2f, 1f, SpriteEffects.None, 0f);
        }

        spriteBatch.End();
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
}
