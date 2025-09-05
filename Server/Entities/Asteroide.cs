using Microsoft.Xna.Framework;

namespace Server.Entities;

public class Asteroid
{
    public Vector2 Position { get; private set; }
    public Vector2 Velocity { get; private set; }
    public float Size { get; }

    public Asteroid(Vector2 position, Vector2 velocity, float size)
    {
        Position = position;
        Velocity = velocity;
        Size = size;
    }

    public void Update() => Position += Velocity;

    public bool CollidesWith(Bullet bullet) => Vector2.Distance(bullet.Position, Position) < Size;
    public bool CollidesWith(Pterosaur player) => Vector2.Distance(player.Position, Position) < Size - 10;
    public Vector2 getPosition() { return Position; }
}
