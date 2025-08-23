using Monogame.Processing;
using Microsoft.Xna.Framework;
using Asteroids;

namespace Screens;

public class MenuScreen
{
    Processing p;
    PImage backgroundImage;
    PImage asteroidSprite;
    const int MAX_ASTEROIDS = 6;
    readonly List<Asteroid> asteroids = new();

    public MenuScreen(Processing p)
    {
        this.p = p;
        backgroundImage = p.loadImage("../Content/menuBackground.png");
        asteroidSprite = p.loadImage("../Content/asteroid.png");
    }

    public void Draw()
    {
        // Fundo
        p.image(backgroundImage, 0, 0, p.width, p.height);

        // Asteroides
        if (p.frameCount % 150 == 0 && asteroids.Count < MAX_ASTEROIDS) asteroids.Add(NovoAsteroid());

        for (int i = asteroids.Count - 1; i >= 0; i--)
        {
            var a = asteroids[i];
            a.Update();
            a.Draw(p);

            // Seo asteroid saiu da tela, remove e cria um novo
            if (a.getPosition().Y > p.height + 50 ||
                a.getPosition().X < -50 ||
                a.getPosition().X > p.width + 50)
            {
                asteroids.RemoveAt(i);
                asteroids.Add(NovoAsteroid());
            }
        }

        // --- título ---
        

        // glow atrás
        p.fill(255, 230, 120, 80); // amarelo transparente
        p.textSize(48);
        // p.text("ASTEROIDS", p.width / 2f + 4, p.height / 3f + 4);

        // título principal
        p.fill(255, 220, 50);
        p.textSize(48);
        // p.text("ASTEROIDS", p.width / 2f, p.height / 3f);

        // --- subtítulo ---
        p.fill(200);
        p.textSize(48);
        // p.text("Prepare-se para a batalha espacial!", p.width / 2f, p.height / 3f - 70);

        // --- mensagem piscando ---
        if (p.frameCount / 30 % 2 == 0) // alterna a cada meio segundo
        {
            p.fill(255, 255, 0);
            p.textSize(48);
            // p.text("Pressione ENTER para jogar", p.width / 2f, p.height - 100);
        }
    }

    private Asteroid NovoAsteroid()
    {
        Random rnd = new Random();

        float x = p.width /2 + rnd.Next(50, 400);
        float y = -30 - rnd.Next(0, 50);

        float targetX = -350;
        float targetY = p.height + 30;

        Vector2 dir = new Vector2(targetX - x, targetY - y);
        dir.Normalize();

        float speed = 0.8f + (float)rnd.NextDouble() * 0.5f;
        float size = 15f + (float)rnd.NextDouble() * 70f;

        return new Asteroid(new Vector2(x, y), dir * speed, size, asteroidSprite);
    }

}
