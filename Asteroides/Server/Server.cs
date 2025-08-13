using System.Net;
using System.Net.Sockets;
using System.Text;

public class Server
{
    private List<TcpClient> players = new List<TcpClient>();
    private TcpListener? listener;

    public async Task StartServer(int port)
    {
        listener = new TcpListener(IPAddress.Any, port);
        listener.Start();

        Console.WriteLine($"Aguardando jogadores na porta {port}...");

        // Aceita novas conexões de forma continua 
        while (true)
        {
            var newClient = await listener.AcceptTcpClientAsync();
            lock (players)
            {
                players.Add(newClient);
            }
            // teste
            Console.WriteLine($"Novo jogador conectado! Total: {players.Count}");
        }
    }

    public async Task<string?> Receive(TcpClient client)
    {
        var stream = client.GetStream();
        var buffer = new byte[32];

        int len = await stream.ReadAsync(buffer, 0, buffer.Length);

        if (len == 0) // cliente desconectou
        {
            lock (players)
            {
                players.Remove(client);
            }
            client.Close();
            // teste
            Console.WriteLine($"Jogador desconectado. Restantes: {players.Count}");
            return null;

        } 
        else 
        {
            string msg = Encoding.ASCII.GetString(buffer, 0, len);
            // teste
            Console.WriteLine($"Recebido: {msg}");
            return Encoding.ASCII.GetString(buffer, 0, len);
        }
    }

    public async Task Send(TcpClient client, string msg)
    {
        var data = Encoding.ASCII.GetBytes(msg);
        var stream = client.GetStream();
        await stream.WriteAsync(data, 0, data.Length);
    }
}