using Monogame.Processing;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System.Text;
using System.Threading.Tasks;
using System.Text.Json;
using Client.Entities;
using Client.Rede;

namespace Client.Screens;

public class ConnectionScreen : IScreen
{
    private GameAsteroids p;
    private PImage backgroundImage;

    private StringBuilder ipInput; 
    private bool isInvalidInput;

    private enum ConnectionOption {Play, Menu, Exit };
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
        isInvalidInput = false;
        p.Window.TextInput += OnTextInput;
    }

    public void LoadContent()
    {
        backgroundImage = p.loadImage("./Content/Scenes/scene4.png");
    }

    public void Draw(JsonElement? state = null)
    {
        p.image(backgroundImage, 0, 0, p.width, p.height);

        p.DrawText("Connection", p.gameFont, new Vector2(p.width / 2f, p.height * 0.4f), Color.Cyan, 1.2f);

        p.DrawText($"Enter the server IP: {(ipInput.Length > 0 ? ipInput : ((p.frameCount / 30) % 2 == 0) ? " " : "|")}", p.gameFont, new Vector2(p.width / 2f, p.height * 0.5f), Color.Cyan, 0.5f);

        if (isInvalidInput) p.DrawText("Enter a valid IP!", p.gameFont, new Vector2(p.width / 2f, p.height * 0.8f), Color.Red, 0.6f);

        Vector2 basePos = new(p.width / 2f, p.height * 0.6f);
        float lineSpacing = 30f;

        for (int i = 0; i < connectionOptions.Length; i++)
        {
            string text;
            
            
                text = string.Join(" ", connectionOptions[i].ToString().Split('_'));

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
                            if (ip.Length == 0) 
                            {
                                isInvalidInput = true;
                            }
                            else 
                            {
                                // Conectar ao servidor de forma assíncrona
                                _ = Task.Run(async () =>
                                {
                                    bool connected = await Connection.GetInstance(p).ConnectToServer(ip);
                                    if (connected)
                                    {
                                        p.setCurrentScreen(ScreenManager.OnlinePlaying);
                                    }
                                    else
                                    {
                                        isInvalidInput = true;
                                    }
                                });
                            }
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
        isInvalidInput = false;
        
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
