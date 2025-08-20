using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;   // só para comparar com Keys.*
using Monogame.Processing;
using Asteroids;
using Screens;
using ComponentsScreens;

public class GameAsteroids : Processing
{
    /* --------------------- estado de jogo --------------------- */
    // estado do jogo: pterosaur, bullets, Asteroids, pontuação
    Pterosaur pterosaur;
    readonly List<Bullet> bullets = new();
    readonly List<Asteroid> asteroids = new();
    readonly Random rnd = new();
    Button playButton;

    /* --------------------- telas de jogo --------------------- */
    MenuScreen menuScreen;

    int score;

    // Controle de tiro
    int fireRate = 15; // frames entre tiros
    int lastShotFrame = 0;

    /* --------------------- teclado (flags) -------------------- */
    bool esquerda, direita, cima, baixo;

    /* ===================== ciclo de vida ====================== */
    public override void Setup()
    {

        size(800, 600);
        pterosaur = new Pterosaur(new Vector2(width / 2f, height - 60), this);
        //menuScreen = new MenuScreen(this, playButton);

        InitializeButtons();

        //menuScreen.Draw();
    }
    
    public void InitializeButtons()
    {
        this.playButton = new Button(width / 2 - 180 / 2, height - 180, 180, 45, new color(121, 81, 38), new color(52, 33, 12), "PLAY", new color(255, 200, 90), 30, this);
    }

    public void GameLoop()
    {
        /* ----- pterosaur ----- */
        Teclas();
        pterosaur.Update(esquerda, direita, cima, baixo, width, height);
        pterosaur.Draw(this);


        /* ----- bullets ----- */
        for (int i = bullets.Count - 1; i >= 0; i--)
        {
            var t = bullets[i];
            t.Update();
            t.Draw(this);
            if (t.OffScreen(height)) bullets.RemoveAt(i);
        }

        /* ----- Asteroids ----- */
        for (int i = asteroids.Count - 1; i >= 0; i--)
        {
            var a = asteroids[i];
            a.Update();
            a.Draw(this);

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
                fill(255, 0, 0);
                textSize(48);
                //textAlign(CENTER, CENTER);
                text("GAME OVER", width / 2f + -4 * 48, height / 2f);
                noLoop();
            }

        proximoAst:;
        }

        /* spawna novo Asteroid a cada 40 quadros */
        if (frameCount % 40 == 0) asteroids.Add(NovoAsteroid());

        /* ----- placar ----- */
        fill(255);
        textSize(20);
        text($"Score: {score}", 10, 10);
    }

    public override void Draw()
    {
        background(0);

        // if (playButton.pressed)
        // {
             GameLoop();
        // }
        // else
        // {
            // menuScreen.Draw();
        // }
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
