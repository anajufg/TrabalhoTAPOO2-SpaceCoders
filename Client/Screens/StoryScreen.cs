using Monogame.Processing;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System.Text.Json;
using System.Text;

namespace Client.Screens;

public class StoryScreen : IScreen
{
    private GameAsteroids p;
    private readonly List<PImage> scenes;
    private readonly List<string> scenesDescription;
    private bool nextScene;
    private int currentSceneIndex;
    private bool wasSpacePressedLastFrame;

    private const int NUM_SCENES = 5;

    public StoryScreen(GameAsteroids p)
    {
        this.p = p;
        scenes = new();
        scenesDescription = new();
        nextScene = false;
        currentSceneIndex = 0;
        wasSpacePressedLastFrame = false;
    }

    public void LoadContent()
    {
        for (int i = 0; i < NUM_SCENES; i++ )
        {
            scenes.Add(p.loadImage($"./Content/Scenes/scene{i+1}.png"));
        }
        scenesDescription.Add("Em um laboratório secreto no ano 3097, cientistas humanos desenvolvem uma máquina do tempo com o objetivo de estudar dinossauros em seu habitat original.");
        scenesDescription.Add("Durante escavações, eles encontram um fóssil intacto de um pterossauro raro, o último de sua linhagem com traços genéticos que indicam uma consciência evoluída e uma missão codificada em seu DNA: impedir o impacto que dizimaria sua espécie.");
        scenesDescription.Add("Ao ser reanimado, o pterossauro desperta antes que os humanos possam controlá-lo.");
        scenesDescription.Add("Ele observa, aprende e se infiltra no laboratório, absorvendo conhecimento e se equipando com a tecnologia avançada: armaduras de carbono, propulsores quânticos, canhões de plasma e um núcleo de dobra temporal.");
        scenesDescription.Add("O pterossauro ativa a máquina do tempo e voa rumo ao passado, direto para o Cretáceo, minutos antes da chuva de asteroides. Seu objetivo: destruir cada rocha cósmica e salvar os dinossauros da extinção.");
    }

    public void Draw(JsonElement? state = null)
    {
        if (currentSceneIndex < 0 || currentSceneIndex >= NUM_SCENES) return;

        p.image(scenes[currentSceneIndex], 0, 0, p.width, p.height);

        float lineSpacing = 25f; 
        string[] wrappedDescription = WrapText(scenesDescription[currentSceneIndex], p.width - 160f);
        float adjustsHeight;
        switch(wrappedDescription.Length)
        {
            case 2:
                adjustsHeight = 62.5f;
                break;
            case 5:
                adjustsHeight = 125f;
                break;
            case 6:
                adjustsHeight = 150f;
                break;
            default:
                adjustsHeight = 100f;
                break;
        }
        
        Vector2 descriptionPos = new(p.width / 2f, p.height - adjustsHeight);
        for (int i = 0; i < wrappedDescription.Length; i++)
        {
            string line = wrappedDescription[i];
            Vector2 linePos = new Vector2(
                descriptionPos.X, 
                descriptionPos.Y + i * lineSpacing
            );
            p.DrawText(line, p.gameFont, linePos, Color.Yellow, 0.55f);
        }

        string next = "Next";
        if (currentSceneIndex == NUM_SCENES - 1) next = "Start";

        Vector2 nextSize = p.gameFont.MeasureString(next);
        Color nextColor = (nextScene && (p.frameCount / 30) % 2 == 0) ? Color.Transparent : Color.White;
        p.DrawText(next, p.gameFont, new Vector2(p.width - 80f, p.height - adjustsHeight - 35f), nextColor, 0.7f);
    }

    public void Update()
    {
        if (p.keyPressed)
        {
            switch (p.keyCode)
            {
                case Keys.Up: 
                    nextScene = false;
                    break;
                case Keys.Down: 
                    nextScene = true;
                    break;
                case Keys.Left: 
                    nextScene = false;
                    break;
                case Keys.Right: 
                    nextScene = true;
                    break;
            }
        }

        bool isSpacePressedNow = p.keyPressed && p.keyCode == Keys.Space;
        
        if (isSpacePressedNow && !wasSpacePressedLastFrame)
        {
            if (nextScene) currentSceneIndex++;
            if (currentSceneIndex >= NUM_SCENES) p.setCurrentScreen(ScreenManager.Playing);
        }
        
        wasSpacePressedLastFrame = isSpacePressedNow;
    }

    public void Reset()
    {
        nextScene = false;
        currentSceneIndex = 0;
        wasSpacePressedLastFrame = false;
    }

    private string[] WrapText(string text, float maxLineWidth)
    {
        string[] words = text.Split(' ');
        List<string> lines = new();
        StringBuilder sb = new();
        float currentLineWidth = 0f;

        foreach (string word in words)
        {
            Vector2 wordSize = p.gameFont.MeasureString(word) * 0.55f; 
            if (currentLineWidth + wordSize.X < maxLineWidth)
            {
                sb.Append(word + " ");
                currentLineWidth += wordSize.X;
            }
            else
            {
                lines.Add(sb.ToString());
                sb.Clear();
                sb.Append(word + " ");
                currentLineWidth = wordSize.X;
            }
        }
        lines.Add(sb.ToString());
        return lines.ToArray();
    }
}