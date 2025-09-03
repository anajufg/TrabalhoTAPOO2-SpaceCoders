
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;
using Server.Entities;
using System;
using System.Collections.Generic;
using System.Numerics;
using Server;

namespace Server.ServerGame
{
    public class ServerGame
    {
        // Game state
        public int level = 0;
        //public ScreenManager currentScreen;

        // Entities
        public List<Asteroid> asteroids = new();
        private const int MAX_ASTEROIDS = 2;

        // Lista de balas
        public List<Bullet> bullets = new();

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
                x = a.getPosition().X,
                y = a.getPosition().Y,
                size = a.size
            });
        }

            // Serializa jogadores com IDs únicos
            var playersState = new List<object>();
            int playerId = 0;
            foreach (var kv in players)
            {
                var p = kv.Value;
                playersState.Add(new
                {
                    id = playerId++,
                    x = p.pos.X,
                    y = p.pos.Y,
                    angle = p.angle // Adicionar o ângulo de rotação
                });
            }

            // Serializa balas
            var bulletsState = new List<object>();
            if (bullets != null)
            {
                foreach (var b in bullets)
                {
                    bulletsState.Add(new {
                        x = b.getPosition().X,
                        y = b.getPosition().Y,
                        vx = b.getVelocity().X,
                        vy = b.getVelocity().Y
                    });
                }
            }

            return new
            {
                asteroids = asteroidsState,
                players = playersState,
                bullets = bulletsState,
                playerCount = players.Count
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
            
            // Atualiza balas apenas se houver balas ativas
            if (bullets.Count > 0)
            {
                UpdateBullets();
            }
        }

        // Atualiza todos os asteroides
        public void UpdateAsteroids()
        {
            if (asteroids.Count < MAX_ASTEROIDS)
                asteroids.Add(NovoAsteroid());

            for (int i = asteroids.Count - 1; i >= 0; i--)
            {
                var a = asteroids[i];
                a.Update();

                // Colisão com jogadores
                foreach (var kv in players)
                {
                    var ptero = kv.Value;
                    if (a.Collide(ptero))
                    {
                        // Remove jogador do jogo
                        players.Remove(kv.Key);
                        break;
                    }
                }

                // Colisão com balas
                for (int j = bullets.Count - 1; j >= 0; j--)
                {
                    var b = bullets[j];
                    if (a.Collide(b))
                    {
                        bullets.RemoveAt(j);
                        asteroids.RemoveAt(i);
                        // Asteroide destruído, não precisa checar offscreen
                        goto NextAsteroid;
                    }
                }

                // Verifica se asteroide saiu da tela
                var apos = a.getPosition();
                if (apos.Y > height + 50 || apos.X < -50 || apos.X > width + 50)
                {
                    asteroids.RemoveAt(i);
                    asteroids.Add(NovoAsteroid());
                }
                NextAsteroid:;
            }
        }

        // Atualiza todas as balas
        public void UpdateBullets()
        {
            // Usar loop reverso para remoção segura
            for (int i = bullets.Count - 1; i >= 0; i--)
            {
                var b = bullets[i];
                b.Update();
                
                // Remove se sair da tela usando método da entidade
                if (b.OffScreen(height))
                {
                    bullets.RemoveAt(i);
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
            speed = 1f + (float)rnd.NextDouble() * 1f;
            size = 15f + (float)rnd.NextDouble() * 35f;
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
            // Posicionar jogadores em posições diferentes para evitar sobreposição
            var playerCount = players.Count;
            Vector2 position;
            
            switch (playerCount)
            {
                case 0: // Primeiro jogador
                    position = new Vector2(width / 4, height / 2);
                    break;
                case 1: // Segundo jogador
                    position = new Vector2(3 * width / 4, height / 2);
                    break;
                case 2: // Terceiro jogador
                    position = new Vector2(width / 2, height / 4);
                    break;
                case 3: // Quarto jogador
                    position = new Vector2(width / 2, 3 * height / 4);
                    break;
                default: // Outros jogadores em posições aleatórias
                    Random rnd = new Random();
                    position = new Vector2(
                        rnd.Next(100, width - 100),
                        rnd.Next(100, height - 100)
                    );
                    break;
            }
            
            players[client] = new Pterosaur(position);
            
            // Inicializar inputs do jogador
            playerInputs[client] = (false, false, false, false);
            
            Console.WriteLine($"Jogador {playerCount + 1} inicializado na posição {position}");
        }

        // Remove um jogador do jogo
        public void RemovePlayer(TcpClient client)
        {
            if (players.ContainsKey(client))
            {
                players.Remove(client);
                playerInputs.Remove(client);
                Console.WriteLine($"Jogador removido do ServerGame. Total restante: {players.Count}");
            }
        }

        private void Reset()
        {
            asteroids.Clear();
            level = 0;
            // Reseta outras coisas se precisar
        }
    }

}
