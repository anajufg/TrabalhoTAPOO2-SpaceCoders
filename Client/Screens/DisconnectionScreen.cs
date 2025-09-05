using Monogame.Processing;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System.Text.Json;
using Client.Entities;

namespace Client.Screens;

public class DisconnectionScreen : IScreen
{
    private GameAsteroids p;
    private PImage backgroundImage;

    private enum DisconnectionOption { Menu, Exit };
    private readonly DisconnectionOption[] disconnectionOptions;
    private int selectedIndex;
    private bool wasKeyPressedLastFrame;

    public DisconnectionScreen(GameAsteroids p)
    {
        this.p = p;
        disconnectionOptions = [DisconnectionOption.Menu, DisconnectionOption.Exit];
        selectedIndex = 0;
        wasKeyPressedLastFrame = false;
    }

    public void LoadContent()
    {
        backgroundImage = p.loadImage("./Content/Backgrounds/disconnection_background.png");
    }

    public void Draw(JsonElement? state = null)
    {
        p.image(backgroundImage, 0, 0, p.width, p.height);

        p.DrawText("Try again", p.gameFont, new Vector2(p.width / 2f, p.height * 0.55f), Color.Yellow, 0.6f);
        
        Vector2 basePos = new(p.width / 2f, p.height * 0.65f);
        float lineSpacing = 30f; 

        for (int i = 0; i < disconnectionOptions.Length; i++)
        {
            string text = disconnectionOptions[i].ToString();
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
                    selectedIndex = (selectedIndex - 1 + disconnectionOptions.Length) % disconnectionOptions.Length;
                    break;
                case Keys.Down: 
                    selectedIndex = (selectedIndex + 1) % disconnectionOptions.Length;
                    break;
                case Keys.Space:
                    DisconnectionOption currentOption = disconnectionOptions[selectedIndex];
                    
                    switch(currentOption) 
                    {
                        case DisconnectionOption.Menu:
                            p.setCurrentScreen(ScreenManager.Menu, true);
                            break;
                        case DisconnectionOption.Exit:
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
