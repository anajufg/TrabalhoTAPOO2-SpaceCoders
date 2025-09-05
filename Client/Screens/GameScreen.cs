using Monogame.Processing;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System.Threading.Tasks;
using System.Text.Json;
using Client.Entities;
using Client.Rede;

namespace Client.Screens;

public class GameScreen : IScreen
{
    private GameAsteroids p;

    private Pterosaur pterosaur;
    private List<Bullet> bullets;
    private List<Asteroid> asteroids;
    private List<Particle> particles;
    private int score;

    private int fireRate;
    private int lastShotFrame;

    private bool isPause;
    private bool wasKeyPressedLastFrame;

    /* ------- Estados do jogador ------- */
    public struct State
    {
        public bool esq, dir, cim, baix, shoot;

        public static bool operator ==(State a, State b)
        {
            return a.esq == b.esq && a.dir == b.dir && a.cim == b.cim && a.baix == b.baix && a.shoot == b.shoot;
        }

        public static bool operator !=(State a, State b)
        {
            return !(a == b);
        }
    }


    State previousState;
    State currentState;

    public GameScreen(GameAsteroids p)
    {
        this.p = p;
    }

    public void LoadContent()
    {
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

    public void Draw(JsonElement? state = null)
    {
        
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
        pterosaur.Update(p.input.esquerda, p.input.direita, p.input.cima, p.input.baixo, p.width, p.height);

        /* ----- enviar ações para o servidor se conectado ----- */
        currentState = new State
        {
            esq = p.input.esquerda,
            dir = p.input.direita,
            cim = p.input.cima,
            baix = p.input.baixo
        };
        previousState = currentState;

        if (Connection.GetInstance(p).IsConnected() && currentState != previousState)
        {
            _ = Task.Run(async () => await Connection.GetInstance(p).SendPlayerAction());
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
                if (!a.Collides(bullets[j])) continue;
                score += 10;
                bullets.RemoveAt(j);
                asteroids.RemoveAt(i);
                goto proximoAst;        // sai dos dois loops
            }

            /* colisão pterosaur × Asteroid */
            if (a.Collides(pterosaur))
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
        p.input.esquerda = false;
        p.input.direita = false;
        p.input.cima = false;
        p.input.baixo = false;
        p.input.shoot = false;
        isPause = false;

        var state = Keyboard.GetState();

        if (state.IsKeyDown(Keys.A)) p.input.esquerda = true;
        if (state.IsKeyDown(Keys.D)) p.input.direita = true;
        if (state.IsKeyDown(Keys.W)) p.input.cima = true;
        if (state.IsKeyDown(Keys.S)) p.input.baixo = true;

        if (state.IsKeyDown(Keys.Space))
        {
            if (p.frameCount - lastShotFrame >= fireRate)
            {
                bullets.Add(pterosaur.Shoot());
                lastShotFrame = p.frameCount;
                p.input.shoot = true;
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
