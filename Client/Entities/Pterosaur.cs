using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;   // só para comparar com Keys.*
using Monogame.Processing;

namespace Client.Entities;

public class Pterosaur
{
    public Vector2 pos;
    Vector2 velocity = Vector2.Zero;
    float angle = -MathF.PI / 2; // Facing up
    const float Acceleration = 0.99f;
    const float MaxSpeed = 6f;
    const float Friction = 0.99f;
    const float RotationSpeed = 0.07f;
    float animation;

    PImage pterosaur;
    const float HalfW = 10, HalfH = 10;

    public Pterosaur(Vector2 start, Processing g)
    {
        pos = start;
        pterosaur = g.loadImage("./Content/Sprites/pterosaur.png");
    }

    public void Update(bool left, bool right, bool up, bool down, int w , int h)
    {
        // Rotation
        if (left) angle -= RotationSpeed;
        if (right) angle += RotationSpeed;

        // Acceleration (thrust)
        if (up)
        {
            velocity.X += MathF.Cos(angle) * Acceleration;
            velocity.Y += MathF.Sin(angle) * Acceleration;
        }
        // Decelerate
        if (down)
        {
            // Aumenta o atrito
            if (velocity.Length() > 0.1f)
                velocity *= 0.92f; 
            // Para completamente
            if (velocity.Length() < 0.15f)
                velocity = Vector2.Zero;
        }

        // Limit speed
        if (velocity.Length() > MaxSpeed)
            velocity = Vector2.Normalize(velocity) * MaxSpeed;

        // Friction
        velocity *= Friction;

        pos += velocity;
        
        // Wrap around screen
        if (pos.X < 0) pos.X += w;
        if (pos.X > w) pos.X -= w;
        if (pos.Y < 0) pos.Y += h;
        if (pos.Y > h) pos.Y -= h;
    }

    public void Draw(Processing g)
    {
        g.push();
        g.translate(pos.X, pos.Y);
        //g.scale((float)1+g.cos(animation)/((velocity.X+velocity.Y+10)/1.0f), 1);
        g.rotate(angle + MathF.PI / 2);
        g.image(this.pterosaur, -100, -50, 200, 100);
        g.pop(); 

        animation+=.1f;
    }

    public static void Draw(GameAsteroids g, Vector2 pos)
    {
        g.push();
        g.translate(pos.X, pos.Y);
        //g.scale((float)1+g.cos(animation)/((velocity.X+velocity.Y+10)/1.0f), 1);
        // g.rotate(angle + MathF.PI / 2);
        g.image(g.pterosaurSprite, -100, -50, 200, 100);
        g.pop(); 

        //animation+=.1f;
        //Console.WriteLine($"pos.X: {pos.X}, pos.Y: {pos.Y}, angle: {angle}");
    }

    public Bullet Shoot()
    {
        // Shoot in the direction the ship is facing
        Vector2 dir = new Vector2(MathF.Cos(angle), MathF.Sin(angle));
        return new Bullet(pos + dir * 24, dir * 8);
    }
}
