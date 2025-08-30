using Monogame.Processing;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System.Text;

namespace Cliente.Screens;

public class PauseScreen
{
    private GameAsteroids p;
    private SpriteFont font;
    private SpriteBatch spriteBatch;

    private enum PauseOption { Continue, Restart, Menu, Exit };
    private readonly PauseOption[] pauseOptions;
    private int selectedIndex;
    private bool wasKeyPressedLastFrame;

    public PauseScreen(GameAsteroids p)
    {
        this.p = p;
        pauseOptions = [PauseOption.Continue, PauseOption.Restart, PauseOption.Menu, PauseOption.Exit];
        selectedIndex = 0;
        wasKeyPressedLastFrame = false;
    }

    public void LoadContent()
    {
        spriteBatch = new SpriteBatch(p.GraphicsDevice);
        font = p.Content.Load<SpriteFont>("PressStart"); 
    }

    public void Draw()
    {
        Vector2 basePos = new(p.width / 2f, p.height * 0.4f);
        float lineSpacing = 40f; 

        for (int i = 0; i < pauseOptions.Length; i++)
        {
            string text = pauseOptions[i].ToString();
            Vector2 textPos = new(basePos.X, basePos.Y + i * lineSpacing);
            
            Color textColor = (i == selectedIndex && (p.frameCount / 30) % 2 == 0) ? Color.Transparent : Color.White;
            
            p.DrawText(text, font, textPos, textColor, 0.5f);
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
                    selectedIndex = (selectedIndex - 1 + pauseOptions.Length) % pauseOptions.Length;
                    break;
                case Keys.Down: 
                    selectedIndex = (selectedIndex + 1) % pauseOptions.Length;
                    break;
                case Keys.Space:
                    PauseOption currentOption = pauseOptions[selectedIndex];
                    
                    switch(currentOption) 
                    {
                        case PauseOption.Continue:
                            p.setCurrentScreen(ScreenManager.Playing);
                            break;
                        case PauseOption.Restart:
                            p.setCurrentScreen(ScreenManager.Playing, true);
                            break;
                        case PauseOption.Menu:
                            p.setCurrentScreen(ScreenManager.Menu, true);
                            break;
                        case PauseOption.Exit:
                            p.Exit();
                            break;
                    }
                    break;
            }
        }
        
        wasKeyPressedLastFrame = isKeyPressedNow;
    }

}