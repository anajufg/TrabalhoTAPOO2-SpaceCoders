using System;
using System.Collections.Generic;
using System.Numerics;

using Server.Entities;

namespace Server
{
    public class ServerGame
    {
        // Game state
        public int level = 0;
        public ScreenManager currentScreen;

    // Entities (example: asteroids)
    public List<Asteroid> asteroids = new();
    private const int MAX_ASTEROIDS = 6;

    // Lista de balas
    public List<Bullet> bullets = new();
        // Atualiza todas as balas
        public void UpdateBullets()
        {
            for (int i = bullets.Count - 1; i >= 0; i--)
            {
                var b = bullets[i];
                b.Update();
                // Remove se sair da tela
                if (b.Position.X < 0 || b.Position.X > width || b.Position.Y < 0 || b.Position.Y > height)
                {
                    bullets.RemoveAt(i);
                }
            }
        }

        // Múltiplos jogadores
        public Dictionary<TcpClient, Pterosaur> players = new();

        // Inputs por jogador
        public Dictionary<TcpClient, (bool esquerda, bool direita, bool cima, bool baixo)> playerInputs = new();

        // Game dimensions
        public int width = 800;
        public int height = 600;

        // Serializa o estado do jogo para enviar aos clientes
        public object GetGameState()
        {
            // Serializa asteroides
            var asteroidsState = new List<object>();
            foreach (var a in asteroids)
            {
                asteroidsState.Add(new
                {
                    x = a.Position.X,
                    y = a.Position.Y,
                    size = a.Size
                });
            }

            // Serializa jogadores
            var playersState = new List<object>();
            foreach (var kv in players)
            {
                var p = kv.Value;
                playersState.Add(new
                {
                    x = p.pos.X,
                    y = p.pos.Y,
                    // tirei o angulo por enquanto
                    // angle = p.Angle 
                });
            }


            // Serializa balas
            var bulletsState = new List<object>();
            if (bullets != null)
            {
                foreach (var b in bullets)
                {
                    bulletsState.Add(new {
                        x = b.Position.X,
                        y = b.Position.Y,
                        vx = b.Velocity.X,
                        vy = b.Velocity.Y
                    });
                }
            }

            return new
            {
                asteroids = asteroidsState,
                players = playersState,
                bullets = bulletsState
            };
        }

        // Game update logic
        public void Update()
        {
            // Atualiza todos os jogadores
            foreach (var kv in players)
            {
                var client = kv.Key;
                var ptero = kv.Value;
                if (playerInputs.TryGetValue(client, out var input))
                {
                    ptero.Update(input.esquerda, input.direita, input.cima, input.baixo, width, height);
                }
            }
            // Atualiza asteroides
            UpdateAsteroids();
            // Atualiza balas
            UpdateBullets();
        }

        // Asteroid logic (no graphics)
        public void UpdateAsteroids()
        {
            if (asteroids.Count < MAX_ASTEROIDS)
                asteroids.Add(NovoAsteroid());

            for (int i = asteroids.Count - 1; i >= 0; i--)
            {
                var a = asteroids[i];
                a.Update();
                if (a.Position.Y > height + 50 ||
                    a.Position.X < -50 ||
                    a.Position.X > width + 50)
                {
                    asteroids.RemoveAt(i);
                    asteroids.Add(NovoAsteroid());
                }
            }
        }

        // Factory for new asteroids
        public Asteroid NovoAsteroid()
        {
            Random rnd = new Random();
            float x = 0, y = 0, speed = 0, size = 0;
            Vector2 dir = Vector2.Zero;
            int edge = rnd.Next((level == 0) ? 4 : 0);
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
            speed = 2f + (float)rnd.NextDouble() * 2f;
            size = 30f + (float)rnd.NextDouble() * 70f;
            dir = Vector2.Normalize(dir);
            return new Asteroid(new Vector2(x, y), dir * speed, size);
        }

        // Recebe input de um jogador específico
        public void ReceiveInput(TcpClient client, bool esquerda, bool direita, bool cima, bool baixo, bool shoot = false)
        {
            playerInputs[client] = (esquerda, direita, cima, baixo);
            //se ele atirou, coloca a bala na lista
            if (shoot && players.ContainsKey(client))
            {
                var bullet = players[client].Shoot();
                if (bullet != null)
                    bullets.Add(bullet);
            }
        }

        // Inicializa o jogador para um cliente
        public void InitPlayer(TcpClient client, Vector2 startPos)
        {
            players[client] = new Pterosaur(startPos);
        }

        private void Reset()
        {
            asteroids.Clear();
            level = 0;
            // Reseta outras coisas se precisar
        }
    }

}
