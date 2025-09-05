using Microsoft.Xna.Framework;

namespace Server.Entities;

public class Bullet
{
    public Vector2 Position { get; private set; }
    public Vector2 Velocity { get; }

    public Bullet(Vector2 position, Vector2 velocity)
    {
        Position = position;
        Velocity = velocity;
    }

    public void Update() => Position += Velocity;

    public bool OffScreen(int height) => Position.Y < -5;

    public Vector2 getPosition() { return Position; }
    public Vector2 getVelocity() { return Velocity; }
}
