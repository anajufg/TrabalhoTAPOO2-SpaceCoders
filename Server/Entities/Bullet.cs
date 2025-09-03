using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;   // só para comparar com Keys.*
using Monogame.Processing;

namespace Server.Entities;

public class Bullet
{
    private Vector2 pos, vel;
    public Bullet(Vector2 p, Vector2 v) { pos = p; vel = v; }

    public void Update() => pos += vel;

    public bool OffScreen(int h) => pos.Y < -5;

    public Vector2 getPosition() => pos;
    public Vector2 getVelocity() => vel;
}

