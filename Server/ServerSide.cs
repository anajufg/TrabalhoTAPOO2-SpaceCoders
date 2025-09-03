using Server;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;
using Server.ServerGame;

namespace Server.ServerSide;

public class ServerSide
{
    private List<TcpClient> players = new List<TcpClient>();
    private TcpListener? listener;
    private int lastPlayerCount = 0;
    private ServerGame.ServerGame game = new ServerGame.ServerGame();
    
    // Dicionário para rastrear última atividade dos clientes
    private Dictionary<TcpClient, DateTime> lastActivity = new Dictionary<TcpClient, DateTime>();
    private const int CLIENT_TIMEOUT_SECONDS = 30; // 30 segundos de timeout

    public async Task StartServer(int port)
    {
        listener = new TcpListener(IPAddress.Parse("127.0.0.1"), port);
        listener.Start();

        Console.WriteLine($"Aguardando jogadores na porta {port}...");

        // Instancia o jogo do servidor
        // problema: talvez o jogo começa antes de ter jogadores
        // game já foi inicializado no construtor

        // Loop de atualização do jogo (menos frequente já que input é processado imediatamente)
        var gameLoop = new System.Timers.Timer(50); // Aumentar para 50ms para reduzir overhead
        gameLoop.Elapsed += async (s, e) =>
        {
            try
            {
                // Só atualizar se não houver input recente
                if (players.Count > 0)
                {
                    game.Update();
                    var gameState = game.GetGameState();
                    if (players.Count != lastPlayerCount){
                        Console.WriteLine($"Enviando estado do jogo para {players.Count} jogadores");
                        lastPlayerCount = players.Count;
                    }
                    await SendAll(gameState);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erro no game loop: {ex.Message}");
            }
        };
        gameLoop.Start();

        // Heartbeat para manter conexões ativas
        var heartbeatTimer = new System.Timers.Timer(10000); // Aumentar para 10 segundos para reduzir overhead
        heartbeatTimer.Elapsed += async (s, e) =>
        {
            try
            {
                if (players.Count > 0)
                {
                    Console.WriteLine($"Enviando heartbeat para {players.Count} jogadores");
                    await SendAll(new { type = "ServerHeartbeat", timestamp = DateTime.UtcNow.Ticks });
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erro no heartbeat: {ex.Message}");
            }
        };
        heartbeatTimer.Start();

        _ = Task.Run(async () =>
        {
           while (true)
            {
                try
                {
                    List<TcpClient> clientsCopy;
                    lock (players)
                    {
                        clientsCopy = players.ToList();
                    }
                    
                    if (clientsCopy.Count > 0)
                    {
                        Console.WriteLine($"Verificando {clientsCopy.Count} clientes...");
                    }
                    
                    foreach (var client in clientsCopy)
                    {
                        try
                        {
                            // Verificação mais robusta de conexão
                            if (!client.Connected || !IsClientAlive(client))
                            {
                                Console.WriteLine($"Cliente desconectado detectado");
                                RemoveClient(client, "desconexão");
                                continue;
                            }

                            // Verificar timeout
                            lock (lastActivity)
                            {
                                if (lastActivity.ContainsKey(client))
                                {
                                    var timeSinceLastActivity = DateTime.UtcNow - lastActivity[client];
                                    if (timeSinceLastActivity.TotalSeconds > CLIENT_TIMEOUT_SECONDS)
                                    {
                                        Console.WriteLine($"Cliente inativo por {timeSinceLastActivity.TotalSeconds:F1} segundos, removendo");
                                        RemoveClient(client, "timeout");
                                        continue;
                                    }
                                }
                            }

                            // Processar input imediatamente se disponível
                            if (client.Available > 0)
                            {
                                var msg = await Receive(client);
                                if (msg != null)
                                {
                                    // Atualizar última atividade
                                    lock (lastActivity)
                                    {
                                        if (lastActivity.ContainsKey(client))
                                            lastActivity[client] = DateTime.UtcNow;
                                    }
                                    
                                    // Processar mensagem imediatamente
                                    await ProcessMessage(client, msg);
                                    
                                    // Atualizar o jogo imediatamente após input
                                    game.Update();
                                    
                                    // Enviar estado atualizado imediatamente
                                    var gameState = game.GetGameState();
                                    await SendAll(gameState);
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"Erro no cliente: {ex.Message}");
                            // Só remover se for um erro definitivo de conexão
                            if (ex.Message.Contains("not allowed on non-connected") || 
                                ex.Message.Contains("disconnected") ||
                                ex.Message.Contains("broken") ||
                                ex.Message.Contains("closed") ||
                                ex.Message.Contains("transport connection") ||
                                ex.Message.Contains("disposed"))
                            {
                                RemoveClient(client, "erro definitivo");
                            }
                            else
                            {
                                Console.WriteLine($"Erro temporário no cliente, mantendo conexão: {ex.Message}");
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Erro no loop principal: {ex.Message}");
                }
                
                // Delay mínimo para não sobrecarregar a CPU
                await Task.Delay(1); // Reduzir para 1ms para máxima responsividade
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
            game.InitPlayer(newClient, System.Numerics.Vector2.Zero); // Posição será calculada automaticamente

            // Registrar atividade inicial
            lock (lastActivity)
            {
                lastActivity[newClient] = DateTime.UtcNow;
            }

            await Send(newClient, new
            {
                type = "Welcome",
                message = "Conectado ao servidor Asteroids!",
                playerId = players.Count
            });
            
            // Log adicional para debug
            Console.WriteLine($"Jogador {players.Count} inicializado no ServerGame. Total de jogadores no jogo: {game.players.Count}");
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
            
            // Log detalhado para debug
            Console.WriteLine($"Bytes recebidos: {BitConverter.ToString(jsonBuffer)}");
            Console.WriteLine($"JSON recebido: {jsonString}");
            
            // Verificar se começa com caracteres válidos de JSON
            if (string.IsNullOrWhiteSpace(jsonString))
            {
                Console.WriteLine("String JSON vazia ou nula");
                return null;
            }
            
            var trimmed = jsonString.TrimStart();
            if (!trimmed.StartsWith("{") && !trimmed.StartsWith("["))
            {
                Console.WriteLine($"JSON inválido - não começa com {{ ou [: '{trimmed.Substring(0, Math.Min(10, trimmed.Length))}'");
                return null;
            }

            using var document = JsonDocument.Parse(jsonString);
            return document.RootElement.Clone();
        }
        catch (JsonException ex)
        {
            Console.WriteLine($"Erro de JSON: {ex.Message}");
            Console.WriteLine($"Posição: Linha {ex.LineNumber}, Byte {ex.BytePositionInLine}");
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

            // Verificar se o cliente ainda está conectado antes de tentar enviar
            if (!client.Connected)
            {
                Console.WriteLine($"Tentativa de envio para cliente desconectado");
                return;
            }

            var stream = client.GetStream();
            var json = JsonSerializer.SerializeToUtf8Bytes(msg);
            var len = BitConverter.GetBytes(json.Length);
            
            // Log detalhado para debug
            var jsonString = JsonSerializer.Serialize(msg);
            Console.WriteLine($"Enviando mensagem: {jsonString}");
            Console.WriteLine($"Tamanho: {json.Length}, Bytes de tamanho: {BitConverter.ToString(len)}");
            
            await stream.WriteAsync(len, 0, len.Length);
            await stream.WriteAsync(json, 0, json.Length);
            await stream.FlushAsync();
            
            Console.WriteLine($"Mensagem enviada com sucesso para cliente");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Erro ao enviar mensagem: {ex.Message}");
            // Re-throw para que SendAll possa tratar erros de conexão
            if (ex.Message.Contains("transport connection") || 
                ex.Message.Contains("disposed") ||
                ex.Message.Contains("cancellation") ||
                ex.Message.Contains("host remoto"))
            {
                throw;
            }
        }
    }
    
    public async Task SendAll(object msg)
    {
        List<TcpClient> playersCopy;
        lock (players)
        {
            playersCopy = players.ToList();
        }
        
        var playersToRemove = new List<TcpClient>();
        
        foreach (var player in playersCopy)
        {
            try
            {
                if (player.Connected)
                {
                    await Send(player, msg);
                }
                else
                {
                    Console.WriteLine($"Cliente não conectado, marcando para remoção");
                    playersToRemove.Add(player);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erro ao enviar para jogador: {ex.Message}");
                // Marcar para remoção se for erro de conexão definitivo
                if (ex.Message.Contains("transport connection") || 
                    ex.Message.Contains("disposed") ||
                    ex.Message.Contains("cancellation") ||
                    ex.Message.Contains("host remoto") ||
                    ex.Message.Contains("not allowed on non-connected") || 
                    ex.Message.Contains("disconnected") ||
                    ex.Message.Contains("broken"))
                {
                    Console.WriteLine($"Erro definitivo detectado, marcando jogador para remoção");
                    playersToRemove.Add(player);
                }
                else
                {
                    Console.WriteLine($"Erro temporário, mantendo jogador: {ex.Message}");
                }
            }
        }
        
        // Remover jogadores desconectados
        if (playersToRemove.Count > 0)
        {
            Console.WriteLine($"Removendo {playersToRemove.Count} jogadores desconectados");
            lock (players)
            {
                foreach (var player in playersToRemove)
                {
                    if (players.Contains(player))
                    {
                        players.Remove(player);
                        game.RemovePlayer(player);
                        player.Close();
                        Console.WriteLine($"Jogador removido por erro de envio. Restantes: {players.Count}");
                    }
                }
            }
        }
    }
     
    private async Task ProcessMessage(TcpClient client, JsonElement? message)
    {
        if (message == null) return;
        try
        {
            // Verificar se é uma mensagem de heartbeat
            if (message.Value.TryGetProperty("type", out var typeProp) &&
                typeProp.ValueKind == JsonValueKind.String)
            {
                string type = typeProp.GetString() ?? "";
                if (type == "Heartbeat")
                {
                    // Responder ao heartbeat para manter a conexão ativa
                    await Send(client, new {
                        type = "HeartbeatAck",
                        timestamp = DateTime.UtcNow.Ticks
                    });
                    return;
                }
            }

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

                    // Atualiza o estado do jogador no game, incluindo tiro
                    game.ReceiveInput(client, esquerda, direita, cima, baixo, tiro);
                    Console.WriteLine($"Input recebido: E:{esquerda} D:{direita} C:{cima} B:{baixo} T:{tiro}");
                }
            }
            
            // Só enviar Ack para mensagens que não são heartbeat
            if (!message.Value.TryGetProperty("type", out var _) || 
                message.Value.GetProperty("type").GetString() != "Heartbeat")
            {
                await Send(client, new {
                    type = "Ack",
                    message = "Mensagem recebida com sucesso"
                });
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Erro ao processar mensagem: {ex.Message}");
        }
    }

    // Método para verificar se o cliente ainda está vivo
    private bool IsClientAlive(TcpClient client)
    {
        try
        {
            // Verificar se o socket ainda está ativo
            if (client.Client == null || !client.Client.Connected)
                return false;

            // Verificar se o stream ainda está disponível
            if (client.GetStream() == null)
                return false;

            // Verificar se o socket não foi fechado
            if (client.Client.Poll(0, SelectMode.SelectRead))
            {
                byte[] buff = new byte[1];
                if (client.Client.Receive(buff, SocketFlags.Peek) == 0)
                {
                    // Cliente fechou a conexão
                    return false;
                }
            }

            return true;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Erro ao verificar se cliente está vivo: {ex.Message}");
            return false;
        }
    }

    private void RemoveClient(TcpClient client, string reason)
    {
        lock (players)
        {
            if (players.Contains(client))
            {
                players.Remove(client);
                game.RemovePlayer(client);
                
                // Remover do rastreamento de atividade
                lock (lastActivity)
                {
                    lastActivity.Remove(client);
                }
                
                client.Close();
                Console.WriteLine($"Cliente removido por {reason}. Restantes: {players.Count}");
            }
        }
    }
}