using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;   // só para comparar com Keys.*
using Monogame.Processing;

namespace Client.Entities;

public class Asteroid
{
    public Vector2 Position { get; private set; }
    public Vector2 Velocity { get; }
    public float Size { get; }
    
    private readonly PImage? sprite;
    private float rotation;
    private readonly float rotationSpeed;

    public Asteroid(Vector2 pos, Vector2 vel, float size, PImage? sprite = null, Processing? g = null, bool randomRotation = true)
    {
        Position = pos;
        Velocity = vel;
        Size = size;
        this.sprite = sprite;

        if (g != null && randomRotation)
        {
            rotation = g.random(-1, 1);
            rotationSpeed = g.random(-1, 1) / 10.0f;
        }
    }

    public void Update()
    {
        Position += Velocity;
        rotation += rotationSpeed;
    }

    public void Draw(Processing g)
    {
        g.push();
        g.translate(Position.X, Position.Y);
        g.rotate(rotation);

        if (sprite != null)
            g.image(sprite, -Size / 2, -Size / 2, Size, Size);
        else
        {
            g.fill(200);
            g.circle(Position.X, Position.Y, Size);
        }

        g.pop();
    }

    public static void Draw(GameAsteroids g, float x, float y, float size)
    {   
        Random rnd = new Random();
        PImage sprite;
        sprite = g.gameAsteroidsSprites[1];
        if (sprite != null)
        {
            g.push();
            g.translate(x, y);
            // g.rotate(rotation);
            g.image(sprite, - size / 2, - size / 2, size, size);
            g.pop();
        }
        else
        {
            g.fill(200);
            g.circle(x, y, size);
        }

        // rotation += rotationSpeed;
    }

    public bool Collides(Bullet b) => Vector2.Distance(b.Position, Position) < Size;
    public bool Collides(Pterosaur p) => Vector2.Distance(p.Position, Position) < Size - 10;
    public Vector2 getPosition() { return Position; }
}
