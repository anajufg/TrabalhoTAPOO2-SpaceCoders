using Client.Rede;
using System.Text.Json;
using Cliente.Screens;

namespace Client.Rede
{
    public sealed class Connection
    {
        /* --------------------- Globals -------------------------------- */
        private GameAsteroids p;
        private string? serverIP;

        private Connection(GameAsteroids p)
        {
            this.p = p;
        }

        private static Connection _instance;

        private static readonly object _lock = new object();

        public static Connection GetInstance(GameAsteroids p)
        {
            if (_instance == null)
            {
                lock (_lock)
                {
                    if (_instance == null)
                    {
                        _instance = new Connection(p);
                    }
                }
            }
            return _instance;
        }
    

        /* --------------------- sistema de rede --------------------- */
        private TcpClientWrapper? networkClient;
        private HandleGame? handleGame;
        private bool isConnected = false;
        private int serverPort = 9000;


        /* --------------------- sistema de rede --------------------- */

        public async Task<bool> ConnectToServer(string serverIP)
        {
            this.serverIP = serverIP;
            try
            {
                networkClient = new TcpClientWrapper();
                handleGame = new HandleGame(networkClient);

                networkClient.OnMessageReceived += OnNetworkMessageReceived;
                networkClient.OnDisconnected += OnNetworkDisconnected;

                await networkClient.ConnectAsync(serverIP, serverPort, p);
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
            if (networkClient != null)
            {
                networkClient.Disconnect();
                networkClient = null;
                handleGame = null;
                isConnected = false;
            }
        }

        private void OnNetworkMessageReceived(JsonElement message)
        {
            try
            {
                //Console.WriteLine($"Mensagem recebida: {message}");
                
                // Processar mensagens do servidor
                if (message.TryGetProperty("type", out var typeProp) && 
                    typeProp.ValueKind == JsonValueKind.String)
                {
                    string messageType = typeProp.GetString() ?? "";
                    
                    switch (messageType)
                    {
                        case "Welcome":
                            if (message.TryGetProperty("message", out var welcomeMsg))
                            {
                                Console.WriteLine($"Servidor: {welcomeMsg.GetString()}");
                            }
                            break;
                            
                        case "Ack":
                            if (message.TryGetProperty("message", out var ackMsg))
                            {
                                Console.WriteLine($"Confirmação: {ackMsg.GetString()}");
                            }
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

            if (p.getCurrentScreen() != ScreenManager.GameOver)
            {
                p.setCurrentScreen(ScreenManager.Disconnection);
            }
        }

        public async Task SendPlayerAction()
        {
 
            if (true)
            {
                try
                {
                    await handleGame.Action(p.esquerda, p.direita, p.cima, p.baixo, p.shoot);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Erro ao enviar ação: {ex.Message}");
                    isConnected = false;
                }
            }
        }

        public bool IsConnected() => isConnected;

        public string? GetServerIP() => serverIP;
    }
}