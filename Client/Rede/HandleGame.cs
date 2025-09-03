using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Client.Rede;

public class HandleGame
{

    TcpClientWrapper client;

    public HandleGame(TcpClientWrapper client)
    {
        this.client = client;
    }

    public async Task Action(bool left, bool right, bool up, bool down)
    {

        Console.WriteLine("Enviando ação do jogador...");
        var msg = new
        {
            Action = "Move",
            Direction = new { Left = left, Right = right, Up = up, Down = down },
        };

        await client.SendAsync(msg);
    }
}
