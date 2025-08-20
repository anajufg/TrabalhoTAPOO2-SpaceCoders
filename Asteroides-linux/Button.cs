using Monogame.Processing;

namespace ComponentsScreens;

public class Button
{
    Processing p;
    int x, y, l, h;
    color cor, corAtual;
    color sombra, sombraAtual;
    string texto;
    color corTexto, corTextoAtual;
    color corHightLight = 0, sombraHightLight = 225, corTextoHightLight = 255;
    int textoSize;
    bool pressed;

    public Button(int x, int y, int l, int h, color cor, color sombra, string texto, color corTexto, int textoSize, Processing p)
    {
        this.p = p;
        this.x = x;
        this.y = y;
        this.l = l;
        this.h = h;
        this.cor = cor;
        this.corAtual = cor;
        this.sombra = sombra;
        this.sombraAtual = sombra;
        this.texto = texto;
        this.corTexto = corTexto;
        this.corTextoAtual = corTexto;
        this.textoSize = textoSize;
        this.pressed = false;
    }

    public void Show() {
        p.noStroke();
    
        // Sombra do botao
        p.fill(sombraAtual);
        p.rect(x, y+h/4, l, h);
        p.rect(x+l, y+h/2, l/16, h/2);
        p.rect(x-l/16, y+h/2, l/16, h/2);

        // Botao
        p.fill(corAtual);
        p.rect(x, y, l, h);
        p.rect(x+l, y+h/4, l/16, h/2);
        p.rect(x-l/16, y+h/4, l/16, h/2);

        // Texto
        // p.textFont(fonte, 50);
        p.fill(corTextoAtual);
        p.textSize(textoSize);
        p.text(texto, x+l/2, y+h/2+h/4);
    }

    public void Select() { 
        // Botao selecionado
        // Verifica se o mouse está em cima do botao
        if (p.mouseX >= x && p.mouseX <= x+l && p.mouseY >= y && p.mouseY <= y+h) {
        corAtual = corHightLight;
        sombraAtual = sombraHightLight;
        corTextoAtual = corTextoHightLight;
        
        if(p.mousePressed) {
            pressed = true;
        } 
        } else {
        corAtual = cor;
        sombraAtual = sombra;
        corTextoAtual = corTexto;
        }
    }
}
