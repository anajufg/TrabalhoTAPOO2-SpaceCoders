using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;   // só para comparar com Keys.*
using Monogame.Processing;

namespace Client.Entities;

class Asteroid
{
    private Vector2 pos, vel;
    private PImage? asteroidSprite;
    public float size { get; }

    public Asteroid(Vector2 p, Vector2 v, float r)
    { pos = p; vel = v; size = r; }

    public Asteroid(Vector2 p, Vector2 v, float r, PImage sprite)
    {
        pos = p; vel = v; size = r;
        asteroidSprite = sprite;
    }

    public void Update() => pos += vel;

    public void Draw(Processing g)
    {
        if (asteroidSprite != null)
        {
            g.image(asteroidSprite, pos.X - size / 2, pos.Y - size / 2, size, size);
        }
        else
        {
            g.fill(200);
            g.circle(pos.X, pos.Y, size);
        }
    }

    public bool Collide(Bullet t) => Vector2.Distance(t.pos, pos) < size;
    public bool Collide(Pterosaur n) => Vector2.Distance(n.pos, pos) < size + 8;

    public Vector2 getPosition() => pos;
}
