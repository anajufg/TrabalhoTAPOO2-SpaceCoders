using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;   // só para comparar com Keys.*
using Monogame.Processing;

namespace Client.Entities;

public class Asteroid
{
    private Vector2 pos, vel;
    private PImage? asteroidSprite;
    float rotation = 0, rotationSpeed = 0;
    public float size { get; }

    public Asteroid(Vector2 p, Vector2 v, float r)
    { pos = p; vel = v; size = r; }

    public Asteroid(Vector2 p, Vector2 v, float r, PImage sprite, Processing g, bool isRotation = true)
    {
        if (isRotation) 
        {
            rotation = g.random(-1, 1);
            rotationSpeed = g.random(-1, 1)/10.0f;
        }

        pos = p; vel = v; size = r;
        asteroidSprite = sprite;
    }

    public void Update() => pos += vel;

    public void Draw(Processing g)
    {   
        if (asteroidSprite != null)
        {
            g.push();
            g.translate(pos.X , pos.Y );
            g.rotate(rotation);
            g.image(asteroidSprite, - size / 2, - size / 2, size, size);
            g.pop();
        }
        else
        {
            g.fill(200);
            g.circle(pos.X, pos.Y, size);
        }

        rotation += rotationSpeed;
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

    public bool Collide(Bullet t) => Vector2.Distance(t.pos, pos) < size;
    public bool Collide(Pterosaur n) => Vector2.Distance(n.pos, pos) < size-10;

    public Vector2 getPosition() => pos;
}
