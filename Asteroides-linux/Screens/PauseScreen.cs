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

    private enum PauseOption { Restart, Exit };
    private PauseOption selectedOption = PauseOption.Restart;

    public PauseScreen(GameAsteroids p)
    {
        this.p = p;
    }

    public void LoadContent()
    {
        spriteBatch = new SpriteBatch(p.GraphicsDevice);
        font = p.Content.Load<SpriteFont>("PressStart"); 
    }

    public void Draw()
    {
        spriteBatch.Begin();

        string restart = "Restart";
        Vector2 restartSize = font.MeasureString(restart);
        Vector2 restartPos = new(p.width / 2f, p.height - 250f); 
        Color restartColor = (selectedOption == PauseOption.Restart && (p.frameCount / 30) % 2 == 0) ? Color.Transparent : Color.White;
        spriteBatch.DrawString(font, restart, restartPos,  restartColor,
            0f, restartSize / 2f, 0.5f, SpriteEffects.None, 0f);

        string exit = "Exit";
        Vector2 exitSize = font.MeasureString(exit);
        Vector2 exitPos = new(p.width / 2f, p.height - 225f); 
        Color exitColor = (selectedOption == PauseOption.Exit && (p.frameCount / 30) % 2 == 0) ? Color.Transparent : Color.White;
        spriteBatch.DrawString(font, exit, exitPos, exitColor,
        0f, exitSize / 2f, 0.5f, SpriteEffects.None, 0f);
        
        spriteBatch.End();
       
    }

    public void Update()
    {
        if (p.keyPressed)
        {
            switch (p.keyCode)
            {
                case Keys.Up: 
                    if (selectedOption == PauseOption.Exit) selectedOption = PauseOption.Restart;
                    break;
                case Keys.Down: 
                    if (selectedOption == PauseOption.Restart) selectedOption = PauseOption.Exit;
                    break;
            }
        }

        if (p.keyPressed && p.keyCode == Keys.Space)
        {
            if (selectedOption == PauseOption.Restart)
            {
                p.setCurrentScreen(ScreenManager.Playing);
            }
            else if (selectedOption == PauseOption.Exit)
            {
                p.setCurrentScreen(ScreenManager.Menu); 
            }
        }
    }

}