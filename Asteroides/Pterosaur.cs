using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;   // só para comparar com Keys.*
using Monogame.Processing;

namespace Asteroids;

class Pterosaur
{
    public Vector2 pos;
    PImage pterosaur;
    const float vel = 4f;
    const float HalfW = 10, HalfH = 10;

    public Pterosaur(Vector2 start, Processing g)
    {
        pos = start;
        pterosaur = g.loadImage("../Content/pterosaur_yellow.png");
    }

    public void Update(bool left, bool right, bool up, bool down, int w, int h)
    {
        Vector2 dir = Vector2.Zero;
        if (left) dir.X -= 2;
        if (right) dir.X += 2;
        if (up) dir.Y -= 2;
        if (down) dir.Y += 2;

        if (dir != Vector2.Zero) dir.Normalize();
        pos += dir * vel;

        /* mantém dentro da tela */
        pos.X = Math.Clamp(pos.X, HalfW, w - HalfW);
        pos.Y = Math.Clamp(pos.Y, HalfH, h - HalfH);
    }

    public void Draw(Processing g)
    {
        g.image(this.pterosaur, pos.X-100, pos.Y-50, 200, 100);
    }

    public Bullet Shoot() => new(pos + new Vector2(0, -12), new Vector2(0, -8));
}
