using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;   // só para comparar com Keys.*
using Monogame.Processing;
using Client.Entities;
using Cliente.Screens;

public class GameAsteroids : Processing
{
    /* --------------------- estado de jogo --------------------- */
    private ScreenManager currentScreen = ScreenManager.Menu;
    
    /* --------------------- elementos d0 jogo --------------------- */
    private Pterosaur pterosaur;
    private readonly List<Bullet> bullets = new();
    private readonly List<Asteroid> asteroids = new();
    private readonly Random rnd = new();
    private int score;

    /* --------------------- telas de jogo --------------------- */
    private MenuScreen menuScreen;
    private StoryScreen storyScreen;

    /* --------------------- configurações de jogo --------------------- */
    private int fireRate = 15; // frames entre tiros
    private int lastShotFrame = 0;

    /* --------------------- teclado (flags) -------------------- */
    private bool esquerda, direita, cima, baixo;

    /* ===================== ciclo de vida ====================== */
    public override void Setup()
    {
        size(800, 600);

        menuScreen = new MenuScreen(this);
        menuScreen.LoadContent();

        storyScreen = new StoryScreen(this);
        storyScreen.LoadContent();

        pterosaur = new Pterosaur(new Vector2(width / 2f, height - 60), this);
    }

    public void Update() 
    {
        switch (currentScreen)
        {
            case ScreenManager.Menu:
                menuScreen.Update();
                break;
            case ScreenManager.Playing:
                GameLoop();
                break;
            case ScreenManager.StoryMode:
                storyScreen.Update(); 
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
                pterosaur.Draw(this);
                for (int i = bullets.Count - 1; i >= 0; i--)
                {
                    bullets[i].Draw(this);
                }
                for (int i = asteroids.Count - 1; i >= 0; i--)
                {
                    asteroids[i].Draw(this);
                }
                fill(255);
                textSize(20);
                text($"Score: {score}", 10, 10);
                break;

            case ScreenManager.StoryMode:
                storyScreen.Draw();
                break;

            case ScreenManager.GameOver:
                fill(255, 0, 0);
                textSize(48);
                text("GAME OVER", width / 2f, height / 2f);
                /* ----- placar ----- */
                fill(255);
                textSize(20);
                text($"Score: {score}", 10, 10);
                break;
        }

        Update();
    }

    public void GameLoop()
    {
        /* ----- pterosaur ----- */
        Teclas();
        pterosaur.Update(esquerda, direita, cima, baixo, width, height);

        /* ----- bullets ----- */
        for (int i = bullets.Count - 1; i >= 0; i--)
        {
            var t = bullets[i];
            t.Update();
            if (t.OffScreen(height)) bullets.RemoveAt(i);
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
                setCurrentScreen(ScreenManager.GameOver);
            }

        proximoAst:;
        }

        /* spawna novo Asteroid a cada 40 quadros */
        if (frameCount % 40 == 0) asteroids.Add(NovoAsteroid());

    }

    public void setCurrentScreen(ScreenManager newScreen) 
    {
        currentScreen = newScreen;
    }

    /* ====================== input ============================= */
    public void Teclas()
    {
        esquerda = false;
        direita = false;
        cima = false;
        baixo = false;

        if (!keyPressed) return;  // nada pressionado

        /* tecla “única” (letras) */
        switch (char.ToUpperInvariant(key))
        {
            case 'A': esquerda = true; break;
            case 'D': direita = true; break;
            case 'W': cima = true; break;
            case 'S': baixo = true; break;
        }

        /* teclas especiais (setas, espaço, esc) */
        switch (keyCode)
        {
            case Keys.Left: esquerda = true; break;
            case Keys.Right: direita = true; break;
            case Keys.Up: cima = true; break;
            case Keys.Down: baixo = true; break;

            case Keys.Space:
                if (frameCount - lastShotFrame >= fireRate)
                {
                    bullets.Add(pterosaur.Shoot());
                    lastShotFrame = frameCount;
                }
                break;
            case Keys.Escape: Exit(); break;
        }
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
        dir.Normalize();
        float speed = 2f + (float)rnd.NextDouble() * 2f; // 2–4 px/frame
        float size = 15f + (float)rnd.NextDouble() * 30f; // 15–45 px radius
        return new Asteroid(new Vector2(x, y), dir * speed, size);
    }

    /* ====================== entry-point ======================= */
    [STAThread]
    static void Main()
    {
        using var jogo = new GameAsteroids();
        jogo.Run();
    }
}
