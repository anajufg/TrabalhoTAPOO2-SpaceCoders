using Monogame.Processing;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Client.Entities;

namespace Cliente.Screens;

public class GameScreen
{
    private GameAsteroids p;
    private PImage backgroundImage;
    private PImage asteroidSprite;
    private SpriteFont font;
    private SpriteBatch spriteBatch;

    private Pterosaur pterosaur;
    private readonly List<Bullet> bullets = new();
    private readonly List<Asteroid> asteroids = new();
    private readonly Random rnd = new();
    private int score;

    private int fireRate;
    private int lastShotFrame;

    private bool isPause;
    private bool wasKeyPressedLastFrame;

    public GameScreen(GameAsteroids p)
    {
        this.p = p;
    }

    public void LoadContent()
    {
        spriteBatch = new SpriteBatch(p.GraphicsDevice);
        font = p.Content.Load<SpriteFont>("PressStart"); 
        backgroundImage = p.loadImage("./Content/Backgrounds/menu_background.png");

        pterosaur = new Pterosaur(new Vector2(p.width / 2f, p.height - 60), p);

        fireRate = 15; // frames entre tiros
        lastShotFrame = 0;

        isPause = false;
        wasKeyPressedLastFrame = false;
    }

    public void Draw()
    {
        p.image(backgroundImage, 0, 0, p.width, p.height);

        pterosaur.Draw(p);

        for (int i = bullets.Count - 1; i >= 0; i--)
        {
            bullets[i].Draw(p);
        }
        for (int i = asteroids.Count - 1; i >= 0; i--)
        {
            asteroids[i].Draw(p);
        }

        spriteBatch.Begin();

        string scoreText = $"Score: {score}";
        Vector2 scoreSize = font.MeasureString(scoreText);
        Vector2 scorePos = new(scoreSize.X / 2f + 5f, 30f); 
        spriteBatch.DrawString(font, scoreText, scorePos, Color.Yellow,
            0f, scoreSize / 2f, 0.6f, SpriteEffects.None, 0f);

        Color pauseColor = (isPause) ? Color.Transparent : ((p.frameCount / 30) % 2 == 0) ? Color.Transparent : Color.LightGray;
        string pause = "Press 'p' for pause";
        Vector2 pauseSize = font.MeasureString(pause);
        Vector2 pausePos = new(p.width - pauseSize.X / 3f, 30f); 
        spriteBatch.DrawString(font, pause, pausePos, pauseColor,
            0f, pauseSize / 2f, 0.5f, SpriteEffects.None, 0f); 
        
        spriteBatch.End();
    }

    public void Update()
    {
        /* ----- pterosaur ----- */
        Teclas();
        pterosaur.Update(p.esquerda, p.direita, p.cima, p.baixo, p.width, p.height);

        /* ----- bullets ----- */
        for (int i = bullets.Count - 1; i >= 0; i--)
        {
            var t = bullets[i];
            t.Update();
            if (t.OffScreen(p.height)) bullets.RemoveAt(i);
        }

        /* ----- Asteroids ----- */
        for (int i = asteroids.Count - 1; i >= 0; i--)
        {
            var a = asteroids[i];
            a.Update();

            /* colisão bullet × Asteroid */
            for (int j = bullets.Count - 1; j >= 0; j--)
            {
                if (!a.Collide(bullets[j])) continue;
                score += 10;
                bullets.RemoveAt(j);
                asteroids.RemoveAt(i);
                goto proximoAst;        // sai dos dois loops
            }

            /* colisão pterosaur × Asteroid */
            if (a.Collide(pterosaur))
            {
                p.setCurrentScreen(ScreenManager.GameOver);
            }

        proximoAst:;
        }

        /* spawna novo Asteroid a cada 40 quadros */
        if (p.frameCount % 40 == 0) asteroids.Add(NovoAsteroid());

    }
        /* ====================== input ============================= */
    public void Teclas()
    {
        p.esquerda = false;
        p.direita = false;
        p.cima = false;
        p.baixo = false;
        isPause = false;

        bool isKeyPressedNow = p.keyPressed;

        if (isKeyPressedNow && !wasKeyPressedLastFrame)
        {
            /* tecla “única” (letras) */
            switch (char.ToUpperInvariant(p.key))
            {
                case 'A': p.esquerda = true; break;
                case 'D': p.direita = true; break;
                case 'W': p.cima = true; break;
                case 'S': p.baixo = true; break;
                case 'P': 
                    isPause = true;
                    p.setCurrentScreen(ScreenManager.PauseScreen); 
                    break;
            }

            /* teclas especiais (setas, espaço, esc) */
            switch (p.keyCode)
            {
                case Keys.Left: p.esquerda = true; break;
                case Keys.Right: p.direita = true; break;
                case Keys.Up: p.cima = true; break;
                case Keys.Down: p.baixo = true; break;

                case Keys.Space:
                    if (p.frameCount - lastShotFrame >= fireRate)
                    {
                        bullets.Add(pterosaur.Shoot());
                        lastShotFrame = p.frameCount;
                    }
                    break;
                case Keys.Escape: p.Exit(); break;
            }
        }
        
        wasKeyPressedLastFrame = isKeyPressedNow;
    }

    /* ====================== fábrica de Asteroids ============= */
    Asteroid NovoAsteroid()
    {
        // Choose a random edge: 0=top, 1=bottom, 2=left, 3=right
        int edge = rnd.Next(4);
        float x = 0, y = 0;
        Vector2 dir = Vector2.Zero;
        switch (edge)
        {
            case 0: // top
                x = rnd.Next(p.width);
                y = -30;
                dir = new Vector2((float)(rnd.NextDouble() - 0.5), 1f);
                break;
            case 1: // bottom
                x = rnd.Next(p.width);
                y = p.height + 30;
                dir = new Vector2((float)(rnd.NextDouble() - 0.5), -1f);
                break;
            case 2: // left
                x = -30;
                y = rnd.Next(p.height);
                dir = new Vector2(1f, (float)(rnd.NextDouble() - 0.5));
                break;
            case 3: // right
                x = p.width + 30;
                y = rnd.Next(p.height);
                dir = new Vector2(-1f, (float)(rnd.NextDouble() - 0.5));
                break;
        }
        dir.Normalize();
        float speed = 2f + (float)rnd.NextDouble() * 2f; // 2–4 px/frame
        float size = 15f + (float)rnd.NextDouble() * 30f; // 15–45 px radius
        return new Asteroid(new Vector2(x, y), dir * speed, size);
    }

    public int getScore() { return this.score; }
}
