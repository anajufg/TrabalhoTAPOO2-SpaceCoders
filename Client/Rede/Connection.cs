using System.Text.Json;
using Client.Screens;

namespace Client.Rede
{
    public sealed class Connection
    {
        private readonly GameAsteroids game;
        private TcpClientWrapper? networkClient;
        private HandleGame? handleGame;

        private bool isConnected;
        private string? serverIP;
        private readonly int serverPort = 9000;

        private Connection(GameAsteroids game)
        {
            this.game = game;
        }

        private static Connection? _instance;
        private static readonly object _lock = new();

        public static Connection GetInstance(GameAsteroids game)
        {
            lock (_lock)
            {
                return _instance ??= new Connection(game);
            }
        }

        public async Task<bool> ConnectToServer(string serverIP)
        {
            this.serverIP = serverIP;
            try
            {
                networkClient = new TcpClientWrapper();
                handleGame = new HandleGame(networkClient);

                networkClient.OnMessageReceived += OnNetworkMessageReceived;
                networkClient.OnDisconnected += OnNetworkDisconnected;

                await networkClient.ConnectAsync(serverIP, serverPort);
                isConnected = true;
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erro ao conectar: {ex.Message}");
                isConnected = false;
                return false;
            }
        }

        public void DisconnectFromServer()
        {
            Console.WriteLine("Desconectando do servidor...");
            networkClient?.Disconnect();
            networkClient = null;
            handleGame = null;
            isConnected = false;
        }

        private void OnNetworkMessageReceived(JsonElement message)
        {
            try
            {
                if (message.TryGetProperty("type", out var typeProp) &&
                    typeProp.ValueKind == JsonValueKind.String)
                {
                    string messageType = typeProp.GetString() ?? "";

                    switch (messageType)
                    {
                        case "Welcome":
                            Console.WriteLine($"Servidor: {message.GetProperty("message").GetString()}");
                            break;

                        case "Ack":
                            Console.WriteLine($"Confirmação: {message.GetProperty("message").GetString()}");
                            break;

                        case "GameState":
                            this.game.currentGameState = message;
                            break;

                        default:
                            Console.WriteLine($"Tipo de mensagem desconhecido: {messageType}");
                            break;
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erro ao processar mensagem: {ex.Message}");
            }
        }

        private void OnNetworkDisconnected()
        {
            Console.WriteLine("Desconectado do servidor");
            isConnected = false;

            if (game.getCurrentScreen() != ScreenManager.GameOver)
            {
                game.setCurrentScreen(ScreenManager.Disconnection);
            }
        }

        public async Task SendPlayerAction()
        {
            if (!isConnected || handleGame == null) return;

            try
            {
                await handleGame.SendPlayerActionAsync(
                    game.input.esquerda, game.input.direita,
                    game.input.cima, game.input.baixo, game.input.shoot
                );
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erro ao enviar ação: {ex.Message}");
                isConnected = false;
            }
        }

        public bool IsConnected() => isConnected;
        public string? GetServerIP() => serverIP;
    }
}
