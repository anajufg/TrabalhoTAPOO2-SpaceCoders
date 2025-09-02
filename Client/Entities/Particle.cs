using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;   // só para comparar com Keys.*
using Monogame.Processing;

namespace Client.Entities;

public class Particle
{
    public Vector2 pos, vel;

    public Particle(Vector2 p, Vector2 v) {
        
        pos = p;
        vel = v;
    }

    public void Update(Processing g){
        pos += vel;
        if(pos.Y>g.height){ 
            pos.X = g.random(g.width);
            pos.Y = g.random(-g.height*2);
        }
    }

    public void Draw(Processing g)
    {   
        g.fill(255, 255, 255);
        g.noStroke();
        g.ellipse(pos.X, pos.Y, 2, 2);
    }
}

