using System;
using System.ComponentModel;
using System.Net.Sockets;
using System.Runtime.Serialization;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;


namespace Client.Rede;

public class TcpClientWrapper
{
    private TcpClient _client;
    private NetworkStream _stream;
    private CancellationTokenSource _cts;
    private Timer _heartbeatTimer;

    public event Action<JsonElement>? OnMessageReceived;
    public event Action? OnDisconnected;

    public async Task ConnectAsync(string ip, int port)
    {
        _client = new TcpClient();
        await _client.ConnectAsync(ip, port);
        _stream = _client.GetStream();
        _cts = new CancellationTokenSource();

        // _ = Task.Run(ReceiveLoop);
    }


    public async Task ConnectAsync(string ip, int port, GameAsteroids g)
    {
        _client = new TcpClient();
        await _client.ConnectAsync(ip, port);
        _stream = _client.GetStream();
        _cts = new CancellationTokenSource();

        // Iniciar heartbeat para manter a conexão estável
        _heartbeatTimer = new Timer(SendHeartbeat, g, TimeSpan.Zero, TimeSpan.FromSeconds(10)); // Aumentar para 10 segundos

        _ = Task.Run(() => ReceiveLoop(g));
    }

    private async void SendHeartbeat(object? state)
    {
        if (_stream != null && !_cts.Token.IsCancellationRequested)
        {
            try
            {
                var heartbeat = new { type = "Heartbeat", timestamp = DateTime.UtcNow.Ticks };
                await MessageFraming.SendMessageAsync(_stream, heartbeat, _cts.Token);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erro no heartbeat: {ex.Message}");
            }
        }
    }

    public async Task SendAsync(object message)
    {
        if (_stream == null) return;
        Console.WriteLine($"Enviando mensagem: {message}");
        await MessageFraming.SendMessageAsync(_stream, message, _cts.Token);
    }

    public async Task ReceiveLoop(GameAsteroids g)
    {
        try
        {
            while (!_cts.Token.IsCancellationRequested)
            {
                var json = await MessageFraming.ReadAsync(_stream, _cts.Token);
                if (json == null) break;

                // Usar o método thread-safe para atualizar o estado
                g.UpdateGameState(json.Value);

                if (json.HasValue)
                {
                    OnMessageReceived?.Invoke(json.Value);
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error in ReceiveLoop: {ex.Message}");
        }
        finally
        {
            OnDisconnected?.Invoke();
            _client?.Close();
        }
    }

    public void Disconnect()
    {
        _cts?.Cancel();
        _heartbeatTimer?.Dispose();
        _client?.Close();
    }

}