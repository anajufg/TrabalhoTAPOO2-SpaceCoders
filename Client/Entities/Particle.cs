using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;   // só para comparar com Keys.*
using Monogame.Processing;

namespace Client.Entities;

public class Particle
{
    public Vector2 Position { get; private set; }
    public Vector2 Velocity { get; }

    public Particle(Vector2 pos, Vector2 vel)
    {
        Position = pos;
        Velocity = vel;
    }

    public void Update(Processing g)
    {
        Position += Velocity;

        if (Position.Y > g.height)
        {
            Position = new Vector2(g.random(g.width), g.random(-g.height * 2));
        }
    }

    public void Draw(Processing g)
    {
        g.fill(250, 250, 250);
        g.noStroke();
        g.ellipse(Position.X, Position.Y, 2, 2);
    }
}
