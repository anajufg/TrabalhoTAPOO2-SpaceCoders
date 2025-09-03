using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;   // só para comparar com Keys.*
using Monogame.Processing;

namespace Client.Entities;

public class Particle
{
    public Vector2 pos, vel;
    private float alpha; // Para suavizar a renderização

    public Particle(Vector2 p, Vector2 v) {
        pos = p;
        vel = v;
        alpha = 255; // Inicialmente totalmente visível
    }

    public void Update(Processing g){
        pos += vel;
        if(pos.Y > g.height){ 
            pos.X = g.random(g.width);
            pos.Y = g.random(-g.height*2);
            alpha = 255; // Reset da transparência
        }
        
        // Suavizar a transparência para evitar piscamentos
        if (alpha > 200) alpha = 200;
    }

    public void Draw(Processing g)
    {   
        g.fill(255, 255, 255, (int)alpha);
        g.noStroke();
        g.ellipse(pos.X, pos.Y, 2, 2);
    }
}

