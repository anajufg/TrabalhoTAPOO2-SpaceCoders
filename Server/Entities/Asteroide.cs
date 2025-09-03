using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;   // só para comparar com Keys.*
using Monogame.Processing;

namespace Server.Entities;

public class Asteroid
{
    private Vector2 pos, vel;
    float rotation = 0, rotationSpeed = 0;
    public float size { get; }

    public Asteroid(Vector2 p, Vector2 v, float r)
    { pos = p; vel = v; size = r; }

    // Rotação e sprite são desnecessários no servidor

    // public Asteroid(Vector2 p, Vector2 v, float r, PImage sprite, Processing g, bool isRotation = true)
    // {
    //     if (isRotation) 
    //     {
    //         rotation = g.random(-1, 1);
    //         rotationSpeed = g.random(-1, 1)/10.0f;
    //     }

    //     pos = p; vel = v; size = r;
    //     asteroidSprite = sprite;
    // }

    public void Update() => pos += vel;

    public bool Collide(Bullet t) => Vector2.Distance(t.getPosition(), pos) < size;
    public bool Collide(Pterosaur n) => Vector2.Distance(n.pos, pos) < size-10;

    public Vector2 getPosition() => pos;
    public Vector2 getVelocity() => vel;
}
