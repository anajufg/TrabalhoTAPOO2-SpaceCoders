using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;   // só para comparar com Keys.*
using Monogame.Processing;

namespace Asteroids;

class Asteroid
{
    Vector2 pos, vel;
    public float radius { get; }

    public Asteroid(Vector2 p, Vector2 v, float r)
    { pos = p; vel = v; radius = r; }

    public void Update() => pos += vel;

    public void Draw(Processing g)
    {
        g.fill(150, 100, 100);
        g.stroke(200);
        g.ellipse(pos.X, pos.Y, radius * 2, radius * 2);
    }

    public bool Collide(Bullet t) => Vector2.Distance(t.pos, pos) < radius;
    public bool Collide(Pterosaur n) => Vector2.Distance(n.pos, pos) < radius + 8;
}
