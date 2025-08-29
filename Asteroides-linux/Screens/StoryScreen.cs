using Monogame.Processing;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace Cliente.Screens;

public class StoryScreen
{
    private GameAsteroids p;
    private readonly List<PImage> scenes = new();
    private readonly List<string> scenesDescription = new();
    private SpriteFont font;
    private SpriteBatch spriteBatch;
    private bool NextScene = false;
    private bool wasSpacePressedLastFrame = false;
    private int currentScene = 0;

    private const int NUM_SCENES = 3;

    public StoryScreen(GameAsteroids p)
    {
        this.p = p;
    }

    public void LoadContent()
    {
        spriteBatch = new SpriteBatch(p.GraphicsDevice);
        font = p.Content.Load<SpriteFont>("PressStart"); 
        for (int i = 0; i < NUM_SCENES; i++ )
        {
            scenes.Add(p.loadImage($"./Content/Scenes/scene{i+1}.png"));
        }
        // scenesDescription.Add("Em um laboratório secreto no ano 3097, cientistas humanos desenvolvem uma máquina do tempo com o objetivo de estudar dinossauros em seu habitat original.");
        // scenesDescription.Add("Durante escavações, eles encontram um fóssil intacto de um pterossauro raro, o último de sua linhagem com traços genéticos que indicam uma consciência evoluída e uma missão codificada em seu DNA: impedir o impacto que dizimaria sua espécie.");
        // scenesDescription.Add("Ao ser reanimado, o pterossauro desperta antes que os humanos possam controlá-lo. Ele observa, aprende e se infiltra no laboratório, absorvendo conhecimento e se equipando com a tecnologia avançada: armaduras de carbono, propulsores quânticos, canhões de plasma e um núcleo de dobra temporal.");
        // scenesDescription.Add("O pterossauro ativa a máquina do tempo e voa rumo ao passado, direto para o Cretáceo, minutos antes da chuva de asteroides. Seu objetivo: destruir cada rocha cósmica e salvar os dinossauros da extinção.");
        scenesDescription.Add("teste");
        scenesDescription.Add("outro");
        scenesDescription.Add("e mais um");
        scenesDescription.Add("e e isso");

    }

    public void Draw()
    {
        if (currentScene < 0 || currentScene >= NUM_SCENES) return;

        p.image(scenes[currentScene], 0, 0, p.width, p.height);

        spriteBatch.Begin();

        string description = scenesDescription[currentScene];
        Vector2 descriptionSize = font.MeasureString(description);
        Vector2 descriptionPos = new(p.width / 2f, p.height - 100f); 
        spriteBatch.DrawString(font, description, descriptionPos, Color.Yellow,
            0f, descriptionSize / 2f, 0.7f, SpriteEffects.None, 0f);

        string next = "Next";
        if (currentScene == scenes.Count - 1)
        {
            next = "Start";
        }
    
        Vector2 nextSize = font.MeasureString(next);
        Vector2 nextPos = new(p.width - 50f, p.height - 100f); 
        Color nextColor = (NextScene && (p.frameCount / 30) % 2 == 0) ? Color.Transparent : Color.White;
        spriteBatch.DrawString(font, next, nextPos,  nextColor,
            0f, nextSize / 2f, 0.7f, SpriteEffects.None, 0f);
        
        spriteBatch.End();
       
    }

    public void Update()
    {
        if (p.keyPressed)
        {
            switch (p.keyCode)
            {
                case Keys.Up: 
                    NextScene = false;
                    break;
                case Keys.Down: 
                    NextScene = true;
                    break;
                case Keys.Left: 
                    NextScene = false;
                    break;
                case Keys.Right: 
                    NextScene = true;
                    break;
            }
        }

        bool isSpacePressedNow = p.keyPressed && p.keyCode == Keys.Space;
        
        if (isSpacePressedNow && !wasSpacePressedLastFrame)
        {
            if (NextScene)
            {
                currentScene++;
            }

            if (currentScene >= scenesDescription.Count)
            {
                p.setCurrentScreen(ScreenManager.Playing);
            }
        }
        
        wasSpacePressedLastFrame = isSpacePressedNow;
    
    }
}