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

    private enum PauseOption { Continue, Restart, Exit };
    private readonly PauseOption[] pauseOptions;
    private int selectedIndex;
    private bool wasKeyPressedLastFrame;

    public PauseScreen(GameAsteroids p)
    {
        this.p = p;
        pauseOptions = [PauseOption.Continue, PauseOption.Restart, PauseOption.Exit];
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
        spriteBatch.Begin();

        Vector2 basePos = new(p.width / 2f, p.height / 2f);
        float lineSpacing = 30f; 

        for (int i = 0; i < pauseOptions.Length; i++)
        {
            string text = pauseOptions[i].ToString();
            Vector2 textSize = font.MeasureString(text);
            Vector2 textPos = new(basePos.X, basePos.Y + i * lineSpacing);
            
            Color textColor = (i == selectedIndex && (p.frameCount / 30) % 2 == 0) ? Color.Transparent : Color.White;
            
            spriteBatch.DrawString(font, text, textPos, textColor,
                0f, textSize / 2f, 0.5f, SpriteEffects.None, 0f);
        }
        
        spriteBatch.End();
       
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