using System.Net.Sockets;
using System.Text.Json;

namespace Client.Rede
{
    public class TcpClientWrapper
    {
        private TcpClient? client;
        private NetworkStream? stream;
        private CancellationTokenSource? cts;

        public event Action<JsonElement>? OnMessageReceived;
        public event Action? OnDisconnected;

        public async Task ConnectAsync(string ip, int port)
        {
            client = new TcpClient();
            await client.ConnectAsync(ip, port);
            stream = client.GetStream();
            cts = new CancellationTokenSource();

            _ = Task.Run(ReceiveLoop);
        }

        public async Task SendAsync(object message)
        {
            if (stream == null || cts == null) return;
            await MessageFraming.SendMessageAsync(stream, message, cts.Token);
        }

        private async Task ReceiveLoop()
        {
            try
            {
                while (cts != null && !cts.Token.IsCancellationRequested)
                {
                    if (stream == null) break;
                    var json = await MessageFraming.ReadAsync(stream, cts.Token);
                    if (json == null) break;

                    OnMessageReceived?.Invoke(json.Value);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erro no ReceiveLoop: {ex.Message}");
            }
            finally
            {
                OnDisconnected?.Invoke();
                client?.Close();
            }
        }

        public void Disconnect()
        {
            cts?.Cancel();
            client?.Close();
        }
    }
}
