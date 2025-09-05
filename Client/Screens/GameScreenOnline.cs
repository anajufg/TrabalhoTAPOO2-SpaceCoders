using Monogame.Processing;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Client.Entities;
using System.Threading.Tasks;
using System.Text.Json;
using Client.Rede;

namespace Client.Screens;

public class GameScreenOnline : IScreen
{
    private GameAsteroids p;

    private State previousState;
    private State currentState;

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

    public GameScreenOnline(GameAsteroids p)
    {
        this.p = p;
    }

    public void LoadContent()
    {
        particles = new();

        for(int i = 0; i < 1000; ++i){
            particles.Add(new Particle(new Vector2(p.random(p.width), p.random(-p.height*2, 0)), new Vector2(0, 5+p.random(-2, 2))));
        }
        
        fireRate = 15; // frames entre tiros
        lastShotFrame = 0;

        isPause = false;
        wasKeyPressedLastFrame = false;
    }

    // Recebe o JsonElement do servidor e desenha tudo na tela
    public void Draw(JsonElement? gameState)
    {   
        if (gameState?.ValueKind != JsonValueKind.Object)
        {
            Console.WriteLine("Estado do jogo inválido recebido do servidor.");
            return; 
        }

        for (int i = particles.Count - 1; i >= 0; i--)
        {
            particles[i].Draw(p);
        }

        // Desenha asteroides
        if (gameState?.TryGetProperty("asteroids", out var asteroids) != null)
        {
            foreach (var a in asteroids.EnumerateArray())
            {
                float x = (float)a.GetProperty("x").GetDouble();
                float y = (float)a.GetProperty("y").GetDouble();
                
                float size = (float)a.GetProperty("size").GetDouble();
                Asteroid.Draw(p, x, y, size);
            }
        }

        // Desenha jogadores
        if (gameState?.TryGetProperty("players", out var players) != null)
        {
            foreach (var p in players.EnumerateArray())
            {
                float x = (float)p.GetProperty("x").GetDouble();
                float y = (float)p.GetProperty("y").GetDouble();
                Console.WriteLine(x +", "+y);
                Pterosaur.Draw(this.p, new Vector2(x, y));
            }
        }

        // Desenha balas
        if (gameState?.TryGetProperty("bullets", out var bullets) != null)
        {
            foreach (var b in bullets.EnumerateArray())
            {
                float x = (float)b.GetProperty("x").GetDouble();
                float y = (float)b.GetProperty("y").GetDouble();
                Bullet.Draw(p, new Vector2(x, y));
            }
        }
    }

    public void Update()
    {
        /* ----- enviar ações para o servidor se conectado ----- */
        if (Connection.GetInstance(p).IsConnected())
        {
            _ = Task.Run(async () => await Connection.GetInstance(p).SendPlayerAction());
        }
        Teclas();

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
        isPause = false;
        wasKeyPressedLastFrame = false;
    }

}

