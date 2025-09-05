using Microsoft.Xna.Framework;

namespace Server.Entities;

public class Pterosaur
{
    public Vector2 Position { get; private set; }
    private Vector2 velocity = Vector2.Zero;
    public float Angle { get; private set; } = -MathF.PI / 2;

    private const float Acceleration = 0.99f;
    private const float MaxSpeed = 6f;
    private const float Friction = 0.99f;
    private const float RotationSpeed = 0.07f;

    public Pterosaur(Vector2 startPosition)
    {
        Position = startPosition;
    }

    public void Update(bool left, bool right, bool up, bool down, int width, int height)
    {
        if (left) Angle -= RotationSpeed;
        if (right) Angle += RotationSpeed;

        if (up)
        {
            velocity.X += MathF.Cos(Angle) * Acceleration;
            velocity.Y += MathF.Sin(Angle) * Acceleration;
        }

        if (down)
        {
            if (velocity.Length() > 0.1f) velocity *= 0.92f;
            if (velocity.Length() < 0.15f) velocity = Vector2.Zero;
        }

        if (velocity.Length() > MaxSpeed) velocity = Vector2.Normalize(velocity) * MaxSpeed;

        velocity *= Friction;
        Position += velocity;

        Vector2 pos = Position;

        if (pos.X < 0) pos.X += width;
        if (pos.X > width) pos.X -= width;
        if (pos.Y < 0) pos.Y += height;
        if (pos.Y > height) pos.Y -= height;

        Position = pos;
    }

    public Bullet Shoot()
    {
        Vector2 direction = new Vector2(MathF.Cos(Angle), MathF.Sin(Angle));
        return new Bullet(Position + direction * 24, direction * 8);
    }
}
