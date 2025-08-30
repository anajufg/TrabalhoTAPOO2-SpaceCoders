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

        p.DrawText("Jurassic Impact", font, new Vector2(p.width / 2f, p.height * 0.4f), Color.Yellow, 1f);
        p.DrawText("Prepare-se para mudar o passado!", font, new Vector2(p.width / 2f, p.height * 0.46f), Color.LightGray, 0.7f);
      
        Vector2 basePos = new(p.width / 2f, p.height * 0.6f);
        float lineSpacing = 30f; 

        for (int i = 0; i < menuOptions.Length; i++)
        {
            string text = menuOptions[i].ToString();
            Color textColor = (i == selectedIndex && (p.frameCount / 30) % 2 == 0) ? Color.Transparent : Color.White;
            Vector2 textPos = new(basePos.X, basePos.Y + i * lineSpacing);
            
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
