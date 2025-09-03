using Monogame.Processing;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Client.Entities;
using System.Threading.Tasks;
using System.Text.Json;

using Client.Rede;

namespace Cliente.Screens;

public class GameScreenOnline
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
    public void Draw(JsonElement gameState)
    {   
        if (gameState.ValueKind != JsonValueKind.Object)
        {
            Console.WriteLine("Estado do jogo inválido recebido do servidor.");
            return; // ou desenhe um estado padrão
        }

        // Atualizar e desenhar partículas
        for (int i = particles.Count - 1; i >= 0; i--)
        {
            particles[i].Update(this.p);
            particles[i].Draw(this.p);
        }

        // Desenha asteroides
        if (gameState.TryGetProperty("asteroids", out var asteroids))
        {
            foreach (var a in asteroids.EnumerateArray())
            {
                float x = (float)a.GetProperty("x").GetDouble();
                float y = (float)a.GetProperty("y").GetDouble();
                
                float size = (float)a.GetProperty("size").GetDouble();
                Asteroid.Draw(this.p, x, y, size);
            }
        }

        // Desenha jogadores com identificação
        if (gameState.TryGetProperty("players", out var players))
        {
            foreach (var player in players.EnumerateArray())
            {
                float x = (float)player.GetProperty("x").GetDouble();
                float y = (float)player.GetProperty("y").GetDouble();
                
                // Obter ângulo de rotação do jogador se disponível
                float angle = 0f;
                if (player.TryGetProperty("angle", out var angleProp))
                {
                    angle = (float)angleProp.GetDouble();
                }
                
                // Obter ID do jogador se disponível
                string playerId = "P?";
                if (player.TryGetProperty("id", out var idProp))
                {
                    int id = idProp.GetInt32();
                    playerId = $"P{id + 1}";
                }
                
                // Desenhar o jogador com rotação
                DrawRotatedPlayer(x, y, angle);
                
                // Desenhar ID do jogador acima dele
                this.p.fill(255, 255, 0); // Amarelo para IDs
                this.p.textSize(16);
                this.p.text(playerId, x, y - 60);
            }
            
            // Debug: mostrar número total de jogadores
            if (gameState.TryGetProperty("playerCount", out var playerCount))
            {
                this.p.fill(0, 255, 0); // Verde para contador
                this.p.textSize(20);
                this.p.text($"Jogadores Online: {playerCount}", 10, 30);
            }
        }

        // Desenha balas
        if (gameState.TryGetProperty("bullets", out var bullets))
        {
            foreach (var b in bullets.EnumerateArray())
            {
                float x = (float)b.GetProperty("x").GetDouble();
                float y = (float)b.GetProperty("y").GetDouble();
                Bullet.Draw(this.p, new Vector2(x, y));
            }
        }
    }

    // Método para desenhar jogador com rotação
    private void DrawRotatedPlayer(float x, float y, float angle)
    {
        this.p.push();
        this.p.translate(x, y);
        this.p.rotate(angle + MathF.PI / 2); // Ajustar ângulo para orientação correta
        this.p.image(this.p.pterosaurSprite, -100, -50, 200, 100);
        this.p.pop();
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
        p.esquerda = false;
        p.direita = false;
        p.cima = false;
        p.baixo = false;
        p.shoot = false;
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
                lastShotFrame = p.frameCount;
                p.shoot = true;
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

