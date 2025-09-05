using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;  
using Monogame.Processing;

namespace Client.Entities;

public class Pterosaur
{
    public Vector2 Position;
    private Vector2 Velocity = Vector2.Zero;
    private static float Angle = -MathF.PI / 2; 
    private const float Acceleration = 0.99f;
    private const float MaxSpeed = 6f;
    private const float Friction = 0.99f;
    private const float RotationSpeed = 0.07f;
    private const float ShipWidth = 200f;
    private const float ShipHeight = 100f;

    private static PImage sprite;

    public Pterosaur(Vector2 start, Processing g)
    {
        Position = start;
        sprite = g.loadImage("./Content/Sprites/pterosaur.png");
    }

    public void Update(bool left, bool right, bool up, bool down, int w, int h)
    {
        if (left) Angle -= RotationSpeed;
        if (right) Angle += RotationSpeed;

        if (up)
        {
            Velocity.X += MathF.Cos(Angle) * Acceleration;
            Velocity.Y += MathF.Sin(Angle) * Acceleration;
        }

        if (down)
        {
            if (Velocity.Length() > 0.1f) Velocity *= 0.92f;
            if (Velocity.Length() < 0.15f) Velocity = Vector2.Zero;
        }

        if (Velocity.Length() > MaxSpeed)
            Velocity = Vector2.Normalize(Velocity) * MaxSpeed;

        Velocity *= Friction;
        Position += Velocity;

        Vector2 pos = Position;

        if (pos.X < 0) pos.X += w;
        if (pos.X > w) pos.X -= w;
        if (pos.Y < 0) pos.Y += h;
        if (pos.Y > h) pos.Y -= h;

        Position = pos;
    }

    public void Draw(Processing g)
    {
        g.push();
        g.translate(Position.X, Position.Y);
        g.rotate(Angle + MathF.PI / 2);
        g.image(sprite, -ShipWidth / 2, -ShipHeight / 2, ShipWidth, ShipHeight);
        g.pop();
    }

    public static void Draw(Processing g, Vector2 Position)
    {
        g.push();
        g.translate(Position.X, Position.Y);
        g.rotate(Angle + MathF.PI / 2);
        g.image(sprite, -ShipWidth / 2, -ShipHeight / 2, ShipWidth, ShipHeight);
        g.pop();
    }

    public Bullet Shoot()
    {
        Vector2 dir = new(MathF.Cos(Angle), MathF.Sin(Angle));
        return new Bullet(Position + dir * 24, dir * 8);
    }
}
