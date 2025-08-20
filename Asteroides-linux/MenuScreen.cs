using Monogame.Processing;
using ComponentsScreens;

namespace Screens;

public class MenuScreen
{
    Processing p;
    Button playButton;
    PImage backgroundImage;

    public MenuScreen(Processing p, Button playButton)
    {
        this.p = p;
        backgroundImage = p.loadImage("../Content/menu.png");
        this.playButton = playButton;
    }

    public void Draw()
    {
        
        p.image(backgroundImage, 0, 0, p.width, p.height);
        /* --- título --- */
        // p.fill(0, 200, 255);
        // p.textSize(64);
        // p.text("TITULO", p.width / 2f, p.height / 2f - 40);

        /* --- subtítulo --- */
        // p.textSize(18);
        // p.fill(200);
        // p.text("subtitulo", p.width / 2f, p.height / 2f + 20);

        /* --- botões --- */
        playButton.Show();
        playButton.Select();


        // if (p.frameCount / 30 % 2 == 0)   // pisca a cada 30 frames
        // {
        //     p.textSize(24);
        //     p.fill(255, 255, 0);
        //     p.text("Pressione ENTER para jogar", p.width / 2f, p.height / 2f + 80);
        // }
    }
}
