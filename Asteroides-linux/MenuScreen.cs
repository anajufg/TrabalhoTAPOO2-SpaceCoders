using Monogame.Processing;

namespace Screens;

public static class MenuScreen
{

    public static void Draw(Processing p)
    {
        /* --- título --- */
        p.fill(0, 200, 255);
        p.textSize(64);
        p.text("TITULO", p.width / 2f, p.height / 2f - 40);

        /* --- subtítulo --- */
        p.textSize(18);
        p.fill(200);
        p.text("subtitulo", p.width / 2f, p.height / 2f + 20);

        /* --- botões --- */



        if (p.frameCount / 30 % 2 == 0)   // pisca a cada 30 frames
        {
            p.textSize(24);
            p.fill(255, 255, 0);
            p.text("Pressione ENTER para jogar", p.width / 2f, p.height / 2f + 80);
        }
    }
}
