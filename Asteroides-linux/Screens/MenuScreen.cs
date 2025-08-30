using Monogame.Processing;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Client.Entities;

namespace Cliente.Screens;

public class MenuScreen
{
    private GameAsteroids p;
    private PImage backgroundImage;

    private SpriteFont font;
    private SpriteBatch spriteBatch;

    private enum MenuOption { Play, StoryMode, Exit };
    private readonly MenuOption[] menuOptions;
    private int selectedIndex;
    private bool wasKeyPressedLastFrame;

    private readonly List<Asteroid> asteroids;

    public MenuScreen(GameAsteroids p)
    {
        this.p = p;
        asteroids = new();
        menuOptions = [MenuOption.Play, MenuOption.StoryMode, MenuOption.Exit];
        selectedIndex = 0;
        wasKeyPressedLastFrame = false;
    }

    public void LoadContent()
    {
        spriteBatch = new SpriteBatch(p.GraphicsDevice);
        font = p.Content.Load<SpriteFont>("PressStart"); 
        backgroundImage = p.loadImage("./Content/Backgrounds/menu_background.png");
    }

    public void Draw()
    {
        p.image(backgroundImage, 0, 0, p.width, p.height);

        p.DrawAsteroidsBackground(asteroids);

        spriteBatch.Begin();

        string title = "Jurassic Impact";
        Vector2 titleSize = font.MeasureString(title);
        Vector2 titlePos = new(p.width / 2f, p.height * 0.4f); 
        spriteBatch.DrawString(font, title, titlePos, Color.Yellow,
            0f, titleSize / 2f, 1f, SpriteEffects.None, 0f);

        string subtitle = "Prepare-se para mudar o passado!";
        Vector2 subtitleSize = font.MeasureString(subtitle);
        Vector2 subtitlePos = new(p.width / 2f, titlePos.Y + titleSize.Y + 20f); 
        spriteBatch.DrawString(font, subtitle, subtitlePos, Color.LightGray,
            0f, subtitleSize / 2f, 0.7f, SpriteEffects.None, 0f); 

        Vector2 basePos = new(p.width / 2f, subtitlePos.Y + subtitleSize.Y + 45f);
        float lineSpacing = 30f; 

        for (int i = 0; i < menuOptions.Length; i++)
        {
            string text = menuOptions[i].ToString();
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
                    selectedIndex = (selectedIndex - 1 + menuOptions.Length) % menuOptions.Length;
                    break;
                case Keys.Down: 
                    selectedIndex = (selectedIndex + 1) % menuOptions.Length;
                    break;
                case Keys.Space:
                    MenuOption currentOption = menuOptions[selectedIndex];
                    
                    switch(currentOption) 
                    {
                        case MenuOption.Play:
                            p.setCurrentScreen(ScreenManager.Playing);
                            break;
                        case MenuOption.StoryMode:
                            p.setCurrentScreen(ScreenManager.StoryMode);
                            break;
                        case MenuOption.Exit:
                            p.Exit();
                            break;
                    }
                    break;
            }
        }
        
        wasKeyPressedLastFrame = isKeyPressedNow;
    }
}
