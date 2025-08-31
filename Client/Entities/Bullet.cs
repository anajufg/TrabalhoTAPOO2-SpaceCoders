using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;   // só para comparar com Keys.*
using Monogame.Processing;

namespace Client.Entities;

public class Bullet
{
    public Vector2 pos, vel;
    public Bullet(Vector2 p, Vector2 v) { pos = p; vel = v; }

    public void Update() => pos += vel;

    public void Draw(Processing g)
    {
        g.strokeWeight(5);
        g.stroke(255, 255, 0);
        g.point(pos.X, pos.Y);
        g.strokeWeight(1);
    }

    public bool OffScreen(int h) => pos.Y < -5;
}

