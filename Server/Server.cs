using Server;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;

public class Server
{
    private List<TcpClient> players = new List<TcpClient>();
    private TcpListener? listener;

    public async Task StartServer(int port)
    {
        listener = new TcpListener(IPAddress.Parse("127.0.0.1"), port);
        listener.Start();

        Console.WriteLine($"Aguardando jogadores na porta {port}...");

    // Instancia o jogo do servidor
    var game = new ServerGame();

        // Loop de atualização do jogo
        var gameLoop = new System.Timers.Timer(16);
        gameLoop.Elapsed += (s, e) =>
        {
            game.Update(); // Chama Update do ServerGame, que chama player.Update e UpdateAsteroids
            // Falta serializar e enviar o estado do jogo para os clientes (screenshot)
        };
        gameLoop.Start();

        _ = Task.Run(async () =>
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
                            var msg = await Receive(client);
                            if (msg != null)
                            {
                                Console.WriteLine($"Mensagem recebida: {msg}");
                                await ProcessMessage(client, msg);
                                 // Exemplo: aplicar input recebido no game
                                 // game.ReceiveInput(...);
                            }
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"Erro no cliente: {ex.Message}");
                            lock (players)
                            {
                                players.Remove(client);
                            }
                            client.Close();
                            Console.WriteLine($"Cliente removido devido a erro. Restantes: {players.Count}");
                        }
                    }
                }
                await Task.Delay(10);
            }
        });

        while (true)
        {
            var newClient = await listener.AcceptTcpClientAsync();
            lock (players)
            {
                players.Add(newClient);
            }
            Console.WriteLine($"Novo jogador conectado! Total: {players.Count}");

            // Inicializa o jogador no ServerGame ao conectar
            game.InitPlayer(new System.Numerics.Vector2(game.width / 2, game.height / 2));

            await Send(newClient, new
            {
                type = "Welcome",
                message = "Conectado ao servidor Asteroids!",
                playerId = players.Count
            });
        }

    }

    public async Task<JsonElement?> Receive(TcpClient client)
    {
        try
        {
            var stream = client.GetStream();
            byte[] lenBuf = new byte[4];
            int readLen = await stream.ReadAsync(lenBuf, 0, 4);
            if (readLen == 0) return null; 
            
            int len = BitConverter.ToInt32(lenBuf, 0);
            if (len <= 0 || len > 10000) 
            {
                Console.WriteLine($"Tamanho de mensagem inválido: {len}");
                return null;
            }
            
            byte[] jsonBuffer = new byte[len];
            int readJson = 0;
            while (readJson < len)
            {
                int r = await stream.ReadAsync(jsonBuffer, readJson, len - readJson);
                if (r == 0) return null; 
                readJson += r;
            }

            var jsonString = Encoding.UTF8.GetString(jsonBuffer);
            Console.WriteLine($"JSON recebido: {jsonString}");
            

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

    public async Task Send(TcpClient client, object msg)
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
    
    // public async Task SendAll(string msg)
    // {
    //     var data = Encoding.ASCII.GetBytes(msg);
    //     lock (players)
    //     {
    //         foreach (var player in players.ToList())
    //         {
    //             try
    //             {
    //                 var stream = player.GetStream();
    //                 await stream.WriteAsync(data, 0, data.Length);
    //             }
    //             // caso o player se desconecte
    //             catch
    //             {
    //                 players.Remove(player);
    //             }
    //         }
    //     }
    // }

    private async Task ProcessMessage(TcpClient client, JsonElement? message)
    {
        if (message == null) return;
        try
        {
            // Espera-se que o cliente envie:
            // { Action: "Move", Left: bool, Right: bool, Up: bool, Down: bool, Shoot: bool }
            if (message.Value.TryGetProperty("Action", out var actionProp) &&
                actionProp.ValueKind == JsonValueKind.String)
            {
                string action = actionProp.GetString() ?? "";
                if (action == "Move")
                {
                    bool esquerda = message.Value.TryGetProperty("Left", out var l) && l.GetBoolean();
                    bool direita = message.Value.TryGetProperty("Right", out var r) && r.GetBoolean();
                    bool cima = message.Value.TryGetProperty("Up", out var u) && u.GetBoolean();
                    bool baixo = message.Value.TryGetProperty("Down", out var d) && d.GetBoolean();
                    bool tiro = message.Value.TryGetProperty("Shoot", out var s) && s.GetBoolean();

                    // Atualiza o estado do jogador no game
                    game.ReceiveInput(esquerda, direita, cima, baixo);
                    // Exemplo: se quiser processar tiro, adicione lógica aqui
                    if (tiro)
                    {
                        // game.Atirar(); // Implemente esse método no ServerGame
                    }
                    Console.WriteLine($"Input recebido: E:{esquerda} D:{direita} C:{cima} B:{baixo} T:{tiro}");
                }
            }
            await Send(client, new {
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