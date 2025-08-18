    using System.Net;
    using System.Net.Sockets;
    using System.Text;
    using System.Text.Json;
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

            _ = Task.Run(async () =>
        listener = new TcpListener(IPAddress.Parse("127.0.0.1"), port);
        listener.Start();

        Console.WriteLine($"Aguardando jogadores na porta {port}...");

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
                            Console.WriteLine($"Mensagem recebida: {msg}");
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"Erro no cliente: {ex.Message}");
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
            await Send(newClient,new {
                type = "Input",
                thrust = 1,
                rotate = 0,
                shoot = true,
                dt = 0.016
            });
        }

    }

    public async Task<dynamic?> Receive(TcpClient client)
    {
        var stream = client.GetStream();
        byte[] lenBuf = new byte[4];
        await stream.ReadAsync(lenBuf, 0, 4);
        int len = BitConverter.ToInt32(lenBuf, 0);
        byte[] jsonBuffer = new byte[len];
        int readJson = 0;
        while (readJson < len)
        {
            int r = await stream.ReadAsync(jsonBuffer, readJson, len - readJson);
            if (r == 0) return null;
            readJson += r;
        }

        var json = JsonSerializer.Deserialize<dynamic>(jsonBuffer);

        if (json.ToString() == "sair")
        {
            lock (players)
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
                                Console.WriteLine($"Mensagem recebida: {msg}");
                            }
                            catch (Exception ex)
                            {
                                Console.WriteLine($"Erro no cliente: {ex.Message}");
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
                await Send(newClient,new {
                    type = "Input",
                    thrust = 1,
                    rotate = 0,
                    shoot = true,
                    dt = 0.016
                });
            }

        }

        public async Task<dynamic?> Receive(TcpClient client)
        {
            var stream = client.GetStream();
            byte[] lenBuf = new byte[4];
            await stream.ReadAsync(lenBuf, 0, 4);
            int len = BitConverter.ToInt32(lenBuf, 0);
            byte[] jsonBuffer = new byte[len];
            int readJson = 0;
            while (readJson < len)
            {
                int r = await stream.ReadAsync(jsonBuffer, readJson, len - readJson);
                if (r == 0) return null;
                readJson += r;
            }

            var json = JsonSerializer.Deserialize<dynamic>(jsonBuffer);

            if (json.ToString() == "sair")
            {
                lock (players)
                {
                    players.Remove(client);
                }
                client.Close();
                Console.WriteLine($"Jogador desconectado. Restantes: {players.Count}");
                return null;
            }

            return json;
        }

        public async Task Send(TcpClient client, object msg)
        {
            var stream = client.GetStream();

            var json = JsonSerializer.SerializeToUtf8Bytes(msg);
            var len = BitConverter.GetBytes(json.Length);
            await stream.WriteAsync(len, 0, len.Length);
            await stream.WriteAsync(json, 0, json.Length);
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
    }
