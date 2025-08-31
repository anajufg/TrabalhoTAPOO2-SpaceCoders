using Monogame.Processing;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System.Text;
using Client.Entities;

namespace Cliente.Screens;

public class ConnectionScreen
{
    private GameAsteroids p;
    private PImage backgroundImage;

    private StringBuilder ipInput; 
    private bool isTyping; 

    private enum ConnectionOption { Play, Menu, Exit };
    private readonly ConnectionOption[] connectionOptions;
    private int selectedIndex;
    private bool wasKeyPressedLastFrame;

    public ConnectionScreen(GameAsteroids p)
    {
        this.p = p;
        connectionOptions = [ConnectionOption.Play, ConnectionOption.Menu, ConnectionOption.Exit];
        selectedIndex = 0;
        wasKeyPressedLastFrame = false;

        ipInput = new StringBuilder(); 
        isTyping = true; 
        p.Window.TextInput += OnTextInput;
    }

    public void LoadContent()
    {
        backgroundImage = p.loadImage("./Content/Backgrounds/gameOver_background.png");
    }

    public void Draw()
    {
        p.image(backgroundImage, 0, 0, p.width, p.height);

        p.DrawText("Conexão", p.gameFont, new Vector2(p.width / 2f, p.height * 0.2f), Color.Yellow, 1.5f);

        p.DrawText("Digite o IP do servidor:", p.gameFont, new Vector2(p.width / 2f, p.height * 0.45f), Color.White, 0.5f);
        // if (!isTyping) ipInput = "|";
        p.DrawText($"IP: {ipInput}", p.gameFont, new Vector2(p.width / 2f, p.height * 0.4f), Color.Cyan, 0.5f);

        Vector2 basePos = new(p.width / 2f, p.height * 0.55f);
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

                    switch (currentOption)
                    {
                        case ConnectionOption.Play:
                            string ip = ipInput.ToString();
                            // p.ConnectToServer(ip); // (precisa implementar)
                            p.setCurrentScreen(ScreenManager.Playing);
                            break;
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

    private void OnTextInput(object sender, TextInputEventArgs e)
    {
        if (!isTyping) return;

        char c = e.Character;

        if (char.IsDigit(c) || c == '.')
        {
            ipInput.Append(c);
        }
        else if (c == '\b' && ipInput.Length > 0) 
        {
            ipInput.Remove(ipInput.Length - 1, 1);
        }
    }

    public void Reset()
    {
        selectedIndex = 0;
        wasKeyPressedLastFrame = false;
        ipInput.Clear();
    }
}
