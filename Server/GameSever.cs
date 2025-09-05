using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;
using Server.GameAsteroids;

namespace Server.GameServer;

public class GameServer
{
    private readonly List<TcpClient> players = new();
    private TcpListener? listener;
    private GameAsteroids.GameAsteroids game = null!;

    public async Task StartServer(int port)
    {
        listener = new TcpListener(IPAddress.Loopback, port);
        listener.Start();

        Console.WriteLine($"Servidor aguardando jogadores na porta {port}...");

        game = new GameAsteroids.GameAsteroids();
        StartGameLoop();

        _ = Task.Run(() => ListenForMessagesAsync());

        while (true)
        {
            var newClient = await listener.AcceptTcpClientAsync();
            await HandleNewClientAsync(newClient);
        }
    }

    private void StartGameLoop()
    {
        var gameLoop = new System.Timers.Timer(16);
        gameLoop.Elapsed += (s, e) =>
        {
            game.Update();
            var state = game.GetGameState();
            _ = BroadcastAsync(state);
        };
        gameLoop.Start();
    }

    private async Task ListenForMessagesAsync()
    {
        while (true)
        {
            List<TcpClient> clientsCopy;
            lock (players)
            {
                clientsCopy = players.ToList();
            }

            foreach (var client in clientsCopy)
            {
                if (client.Available > 0)
                {
                    try
                    {
                        var msg = await ReceiveMessageAsync(client);
                        if (msg != null)
                        {
                            Console.WriteLine($"Mensagem recebida: {msg}");
                            await HandleClientMessageAsync(client, msg);
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Erro no cliente: {ex.Message}");
                        DisconnectClient(client);
                    }
                }
            }

            await Task.Delay(10);
        }
    }

    private async Task HandleNewClientAsync(TcpClient client)
    {
        lock (players)
        {
            players.Add(client);
        }

        Console.WriteLine($"Novo jogador conectado! Total: {players.Count}");

        game.InitPlayer(client, new System.Numerics.Vector2(game.Width / 2, game.Height / 2));

        await SendMessageAsync(client, new
        {
            type = "Welcome",
            message = "Conectado ao servidor Asteroids!",
            playerId = players.Count
        });
    }

    private void DisconnectClient(TcpClient client)
    {
        lock (players)
        {
            players.Remove(client);
        }

        client.Close();
        Console.WriteLine($"Cliente desconectado. Restantes: {players.Count}");
    }

    private async Task<JsonElement?> ReceiveMessageAsync(TcpClient client)
    {
        try
        {
            var stream = client.GetStream();

            // lê tamanho da mensagem
            byte[] lenBuf = new byte[4];
            int readLen = await stream.ReadAsync(lenBuf, 0, 4);
            if (readLen == 0) return null;

            int len = BitConverter.ToInt32(lenBuf, 0);
            if (len <= 0 || len > 10000)
            {
                Console.WriteLine($"Tamanho de mensagem inválido: {len}");
                return null;
            }

            // lê conteúdo JSON
            byte[] jsonBuffer = new byte[len];
            int readJson = 0;
            while (readJson < len)
            {
                int r = await stream.ReadAsync(jsonBuffer, readJson, len - readJson);
                if (r == 0) return null;
                readJson += r;
            }

            var jsonString = Encoding.UTF8.GetString(jsonBuffer);
            using var document = JsonDocument.Parse(jsonString);

            return document.RootElement.Clone();
        }
        catch (JsonException ex)
        {
            Console.WriteLine($"Erro de JSON: {ex.Message}");
            return null;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Erro ao receber mensagem: {ex.Message}");
            return null;
        }
    }

    private async Task SendMessageAsync(TcpClient client, object msg)
    {
        try
        {
            var stream = client.GetStream();
            var json = JsonSerializer.SerializeToUtf8Bytes(msg);
            var len = BitConverter.GetBytes(json.Length);

            await stream.WriteAsync(len, 0, len.Length);
            await stream.WriteAsync(json, 0, json.Length);
            await stream.FlushAsync();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Erro ao enviar mensagem: {ex.Message}");
        }
    }

    private bool IsConnected(TcpClient client)
    {
        try
        {
            return !(client.Client.Poll(0, SelectMode.SelectRead) && client.Available == 0);
        }
        catch
        {
            return false;
        }
    }

    private async Task BroadcastAsync(object msg)
    {
        List<TcpClient> clientsCopy;
        lock (players)
        {
            clientsCopy = players.ToList();
        }

        foreach (var client in clientsCopy)
        {
            if (!IsConnected(client))
            {
                DisconnectClient(client);
                continue;
            }

            await SendMessageAsync(client, msg);
        }
    }

    private async Task HandleClientMessageAsync(TcpClient client, JsonElement? message)
    {
        if (message == null) return;

        try
        {
            if (message.Value.TryGetProperty("Action", out var actionProp) &&
                actionProp.ValueKind == JsonValueKind.String)
            {
                var action = actionProp.GetString();
                if (action == "Move")
                {
                    bool left = message.Value.TryGetProperty("Left", out var l) && l.GetBoolean();
                    bool right = message.Value.TryGetProperty("Right", out var r) && r.GetBoolean();
                    bool up = message.Value.TryGetProperty("Up", out var u) && u.GetBoolean();
                    bool down = message.Value.TryGetProperty("Down", out var d) && d.GetBoolean();
                    bool shoot = message.Value.TryGetProperty("Shoot", out var s) && s.GetBoolean();

                    game.ReceiveInput(client, left, right, up, down, shoot);

                    Console.WriteLine($"Input: ←{left} →{right} ↑{up} ↓{down} ●{shoot}");
                }
            }

            await SendMessageAsync(client, new
            {
                type = "Ack",
                message = "Mensagem recebida com sucesso"
            });
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Erro ao processar mensagem: {ex.Message}");
        }
    }
}
