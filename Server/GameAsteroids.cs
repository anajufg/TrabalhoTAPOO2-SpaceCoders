using System.Net.Sockets;
using System.Numerics;
using Server.Entities;

namespace Server.GameAsteroids;

public class GameAsteroids
{
    public int Level { get; private set; } = 0;
    public int Width { get; } = 800;
    public int Height { get; } = 600;

    private const int MaxAsteroids = 1;
    private readonly List<Asteroid> asteroids = new();
    private readonly List<Bullet> bullets = new();
    private readonly Dictionary<TcpClient, Pterosaur> players = new();
    private readonly Dictionary<TcpClient, PlayerInput> playerInputs = new();

    // Serializa o estado atual
    public object GetGameState() => new
    {
        asteroids = asteroids.Select(a => new { x = a.getPosition().X, y = a.getPosition().Y, size = a.size }),
        players = players.Values.Select(p => new { x = p.pos.X, y = p.pos.Y, angle = p.angle }),
        bullets = bullets.Select(b => new { x = b.getPosition().X, y = b.getPosition().Y, vx = b.getVelocity().X, vy = b.getVelocity().Y })
    };

    public void Update()
    {
        UpdatePlayers();
        UpdateAsteroids();
        UpdateBullets();
    }

    private void UpdatePlayers()
    {
        foreach (var (client, ptero) in players)
        {
            if (playerInputs.TryGetValue(client, out var input))
            {
                ptero.Update(input.Left, input.Right, input.Up, input.Down, Width, Height);
            }
        }
    }

    private void UpdateAsteroids()
    {
        if (asteroids.Count < MaxAsteroids)
            asteroids.Add(CreateAsteroid());

        for (int i = asteroids.Count - 1; i >= 0; i--)
        {
            var asteroid = asteroids[i];
            asteroid.Update();

            if (CheckCollisionWithPlayers(asteroid, i)) continue;
            if (CheckCollisionWithBullets(asteroid, i)) continue;
            if (IsOffScreen(asteroid)) ReplaceAsteroid(i);
        }
    }

    private bool CheckCollisionWithPlayers(Asteroid asteroid, int index)
    {
        foreach (var (client, player) in players)
        {
            if (asteroid.Collide(player))
            {
                players.Remove(client);
                return true;
            }
        }
        return false;
    }

    private bool CheckCollisionWithBullets(Asteroid asteroid, int index)
    {
        for (int j = bullets.Count - 1; j >= 0; j--)
        {
            if (asteroid.Collide(bullets[j]))
            {
                bullets.RemoveAt(j);
                asteroids.RemoveAt(index);
                return true;
            }
        }
        return false;
    }

    private bool IsOffScreen(Asteroid asteroid)
    {
        var pos = asteroid.getPosition();
        return pos.Y > Height + 50 || pos.X < -50 || pos.X > Width + 50;
    }

    private void ReplaceAsteroid(int index)
    {
        asteroids.RemoveAt(index);
        asteroids.Add(CreateAsteroid());
    }

    private void UpdateBullets()
    {
        for (int i = bullets.Count - 1; i >= 0; i--)
        {
            var bullet = bullets[i];
            bullet.Update();
            if (bullet.OffScreen(Height))
                bullets.RemoveAt(i);
        }
    }

    private Asteroid CreateAsteroid()
    {
        var rnd = new Random();
        int edge = rnd.Next(Level == 0 ? 4 : 0);
        float x = 0, y = 0;
        Vector2 dir = Vector2.Zero;

        switch (edge)
        {
            case 0: x = rnd.Next(Width); y = -30; dir = new Vector2((float)(rnd.NextDouble() - 0.5), 1f); break;
            case 1: x = rnd.Next(Width); y = Height + 30; dir = new Vector2((float)(rnd.NextDouble() - 0.5), -1f); break;
            case 2: x = -30; y = rnd.Next(Height); dir = new Vector2(1f, (float)(rnd.NextDouble() - 0.5)); break;
            case 3: x = Width + 30; y = rnd.Next(Height); dir = new Vector2(-1f, (float)(rnd.NextDouble() - 0.5)); break;
        }

        float speed = 2f + (float)rnd.NextDouble() * 2f;
        float size = 30f + (float)rnd.NextDouble() * 70f;
        dir = Vector2.Normalize(dir);

        return new Asteroid(new Vector2(x, y), dir * speed, size);
    }

    public void ReceiveInput(TcpClient client, bool left, bool right, bool up, bool down, bool shoot = false)
    {
        playerInputs[client] = new PlayerInput(left, right, up, down);

        if (shoot && players.ContainsKey(client))
        {
            var bullet = players[client].Shoot();
            if (bullet != null) bullets.Add(bullet);
        }
    }

    public void InitPlayer(TcpClient client, Vector2 startPos) => players[client] = new Pterosaur(startPos);

    private record PlayerInput(bool Left, bool Right, bool Up, bool Down);
}
