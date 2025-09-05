using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;   // só para comparar com Keys.*
using Monogame.Processing;

namespace Client.Entities;

public class Bullet
{
    public Vector2 Position { get; private set; }
    public Vector2 Velocity { get; }

    public Bullet(Vector2 pos, Vector2 vel)
    {
        Position = pos;
        Velocity = vel;
    }

    public void Update() => Position += Velocity;

    public void Draw(Processing g)
    {
        g.strokeWeight(5);
        g.stroke(255, 255, 0);
        g.point(Position.X, Position.Y);
        g.strokeWeight(1);
    }

    public static void Draw(Processing g, Vector2 Position)
    {
        g.strokeWeight(5);
        g.stroke(255, 255, 0);
        g.point(Position.X, Position.Y);
        g.strokeWeight(1);
    }

    public bool OffScreen(int screenHeight) => Position.Y < -5 || Position.Y > screenHeight + 5;
}
