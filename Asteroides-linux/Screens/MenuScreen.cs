using Monogame.Processing;

namespace Screens;

public class MenuScreen
{
    Processing p;
    PImage backgroundImage;

    public MenuScreen(Processing p)
    {
        this.p = p;
        backgroundImage = p.loadImage("../Content/menuBackground.png");
    }

    public void Draw()
    {
        // Fundo
        p.image(backgroundImage, 0, 0, p.width, p.height);

        // --- título ---
        

        // glow atrás
        p.fill(255, 230, 120, 80); // amarelo transparente
        p.textSize(48);
        p.text("ASTEROIDS", p.width / 2f + 4, p.height / 3f + 4);

        // título principal
        p.fill(255, 220, 50);
        p.textSize(48);
        p.text("ASTEROIDS", p.width / 2f, p.height / 3f);

        // --- subtítulo ---
        p.fill(200);
        p.textSize(48);
        p.text("Prepare-se para a batalha espacial!", p.width / 2f, p.height / 3f - 70);

        // --- mensagem piscando ---
        if (p.frameCount / 30 % 2 == 0) // alterna a cada meio segundo
        {
            p.fill(255, 255, 0);
            p.textSize(48);
            p.text("Pressione ENTER para jogar", p.width / 2f, p.height - 100);
        }
    }
}
