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

    // Múltiplos jogadores
    public Dictionary<TcpClient, Pterosaur> players = new();

    // Inputs por jogador
    public Dictionary<TcpClient, (bool esquerda, bool direita, bool cima, bool baixo)> playerInputs = new();

        // Game dimensions
        public int width = 800;
        public int height = 600;


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
        public void ReceiveInput(TcpClient client, bool esquerda, bool direita, bool cima, bool baixo)
        {
            playerInputs[client] = (esquerda, direita, cima, baixo);
        }

        // Inicializa o jogador para um cliente
        public void InitPlayer(TcpClient client, Vector2 startPos)
        {
            players[client] = new Pterosaur(startPos);
        }

        // Example: change screen
        public void SetCurrentScreen(ScreenManager newScreen, bool restart = false)
        {
            if ((newScreen == ScreenManager.Playing || newScreen == ScreenManager.Menu) && restart)
            {
                Reset();
            }
            currentScreen = newScreen;
        }

        private void Reset()
        {
            asteroids.Clear();
            level = 0;
            // Reset other game state as needed
        }
    }

    // Minimal asteroid class for server
    public class Asteroid
    {
        public Vector2 Position;
        public Vector2 Velocity;
        public float Size;

        public Asteroid(Vector2 position, Vector2 velocity, float size)
        {
            Position = position;
            Velocity = velocity;
            Size = size;
        }

        public void Update()
        {
            Position += Velocity;
        }
    }
}
