using Monogame.Processing;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Client.Entities;
using System.Threading.Tasks;

namespace Cliente.Screens;

public class GameScreen
{
    private GameAsteroids p;
    private PImage backgroundImage;

    private Pterosaur pterosaur;
    private List<Bullet> bullets;
    private List<Asteroid> asteroids;
    private List<Particle> particles;
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
        backgroundImage = p.loadImage("./Content/Backgrounds/game_background.png");

        pterosaur = new Pterosaur(new Vector2(p.width / 2f, p.height - 60), p);
        particles = new();

        for(int i = 0; i < 1000; ++i){
            particles.Add(new Particle(new Vector2(p.random(p.width), p.random(-p.height*2, 0)), new Vector2(0, 5+p.random(-2, 2))));
        }
        
        bullets = new();
        asteroids = new();

        fireRate = 15; // frames entre tiros
        lastShotFrame = 0;
        score = 0;

        isPause = false;
        wasKeyPressedLastFrame = false;
    }

    public void Draw()
    {
        //p.image(backgroundImage, 0, 0, p.width, p.height);
        //p.background(0);
        for (int i = particles.Count - 1; i >= 0; i--)
        {
            particles[i].Draw(p);
        }
        pterosaur.Draw(p);

        for (int i = bullets.Count - 1; i >= 0; i--)
        {
            bullets[i].Draw(p);
        }
        for (int i = asteroids.Count - 1; i >= 0; i--)
        {
            asteroids[i].Draw(p);
        }
        

        

        string scoreText = $"Score: {score}";
        Vector2 scoreSize = p.gameFont.MeasureString(scoreText);
        p.DrawText(scoreText, p.gameFont, new Vector2(scoreSize.X / 2f + 5f, 30f), Color.Yellow, 0.6f);
        
        string pauseText = "Press ESC for pause";
        Vector2 pauseSize = p.gameFont.MeasureString(pauseText);
        Color pauseColor = (isPause) ? Color.Transparent : ((p.frameCount / 30) % 2 == 0) ? Color.Transparent : Color.LightGray;
        p.DrawText(pauseText, p.gameFont, new Vector2(p.width - pauseSize.X / 3f, 30f), pauseColor, 0.5f);

    }

    public void Update()
    {
        /* ----- pterosaur ----- */
        Teclas();
        pterosaur.Update(p.esquerda, p.direita, p.cima, p.baixo, p.width, p.height);
        
        /* ----- enviar ações para o servidor se conectado ----- */
        if (p.IsConnected())
        {
            _ = Task.Run(async () => await p.SendPlayerAction());
        }

        /* ----- bullets ----- */
        for (int i = bullets.Count - 1; i >= 0; i--)
        {
            var t = bullets[i];
            t.Update();
            if (t.OffScreen(p.height)) bullets.RemoveAt(i);
        }

        for (int i = particles.Count - 1; i >= 0; i--)
        {
            var t = particles[i];
            t.Update(p);
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
        if (p.frameCount % 40 == 0) asteroids.Add(p.NovoAsteroid(true));

    }
    
    /* ====================== input ============================= */
    public void Teclas()
    {
        p.esquerda = false;
        p.direita = false;
        p.cima = false;
        p.baixo = false;
        isPause = false;

        var state = Keyboard.GetState();

        if (state.IsKeyDown(Keys.A)) p.esquerda = true;
        if (state.IsKeyDown(Keys.D)) p.direita = true;
        if (state.IsKeyDown(Keys.W)) p.cima = true;
        if (state.IsKeyDown(Keys.S)) p.baixo = true;

        if (state.IsKeyDown(Keys.Space))
        {
            if (p.frameCount - lastShotFrame >= fireRate)
            {
                bullets.Add(pterosaur.Shoot());
                lastShotFrame = p.frameCount;
            }
        }

        if (state.IsKeyDown(Keys.Escape))
        {
            isPause = true;
            p.setCurrentScreen(ScreenManager.PauseScreen);
        }
    }


    public void Reset()
    {
        pterosaur = new Pterosaur(new Vector2(p.width / 2f, p.height - 60), p);
        bullets = new();
        asteroids = new();
        score = 0;
        isPause = false;
        wasKeyPressedLastFrame = false;
    }

    public int getScore() { return this.score; }
}
