using Microsoft.Xna.Framework.Input;

public class InputState
{
    public bool esquerda;
    public bool direita;
    public bool cima;
    public bool baixo;
    public bool shoot;

    public void KeyPressed(Keys key)
    {
        switch (key)
        {
            case Keys.A:
            case Keys.Left: esquerda = true; break;
            case Keys.D:
            case Keys.Right: direita = true; break;
            case Keys.W:
            case Keys.Up: cima = true; break;
            case Keys.S:
            case Keys.Down: baixo = true; break;
            case Keys.Space: shoot = true; break;
        }
    }

    public void KeyReleased(Keys key)
    {
        switch (key)
        {
            case Keys.A:
            case Keys.Left: esquerda = false; break;
            case Keys.D:
            case Keys.Right: direita = false; break;
            case Keys.W:
            case Keys.Up: cima = false; break;
            case Keys.S:
            case Keys.Down: baixo = false; break;
            case Keys.Space: shoot = false; break;
        }
    }
}
