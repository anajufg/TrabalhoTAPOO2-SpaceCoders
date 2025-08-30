using Monogame.Processing;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Client.Entities;

namespace Cliente.Screens;

public class ConnectionScreen
{
    private GameAsteroids p;
    private PImage backgroundImage;

    private enum ConnectionOption { Menu, Exit };
    private readonly ConnectionOption[] connectionOptions;
    private int selectedIndex;
    private bool wasKeyPressedLastFrame;

    public ConnectionScreen(GameAsteroids p)
    {
        this.p = p;
        connectionOptions = [ConnectionOption.Menu, ConnectionOption.Exit];
        selectedIndex = 0;
        wasKeyPressedLastFrame = false;
    }

    public void LoadContent()
    {
        backgroundImage = p.loadImage("./Content/Backgrounds/gameOver_background.png");
    }

    public void Draw()
    {
        p.image(backgroundImage, 0, 0, p.width, p.height);

        p.DrawText("Conexão", p.gameFont, new Vector2(p.width / 2f, p.height * 0.2f), Color.Yellow, 1.5f);
        p.DrawText("Pipipopo", p.gameFont, new Vector2(p.width / 2f, p.height * 0.30f), Color.White, 0.8f);
        
        Vector2 basePos = new(p.width / 2f, p.height * 0.5f);
        float lineSpacing = 30f; 

        for (int i = 0; i < connectionOptions.Length; i++)
        {
            string text = connectionOptions[i].ToString();
            Vector2 textPos = new(basePos.X, basePos.Y + i * lineSpacing);
            Color textColor = (i == selectedIndex && (p.frameCount / 30) % 2 == 0) ? Color.Transparent : Color.LightGray;
            
            p.DrawText(text, p.gameFont, textPos, textColor, 0.5f);
        }
        
    }

    public void Update()
    {
        bool isKeyPressedNow = p.keyPressed;

        if (isKeyPressedNow && !wasKeyPressedLastFrame)
        {
            switch (p.keyCode)
            {
                case Keys.Up: 
                    selectedIndex = (selectedIndex - 1 + connectionOptions.Length) % connectionOptions.Length;
                    break;
                case Keys.Down: 
                    selectedIndex = (selectedIndex + 1) % connectionOptions.Length;
                    break;
                case Keys.Space:
                    ConnectionOption currentOption = connectionOptions[selectedIndex];
                    
                    switch(currentOption) 
                    {
                        case ConnectionOption.Menu:
                            p.setCurrentScreen(ScreenManager.Menu, true);
                            break;
                        case ConnectionOption.Exit:
                            p.Exit();
                            break;
                    }
                    break;
            }
        }
        
        wasKeyPressedLastFrame = isKeyPressedNow;
    }

    public void Reset()
    {
        selectedIndex = 0;
        wasKeyPressedLastFrame = false;
    }
}
