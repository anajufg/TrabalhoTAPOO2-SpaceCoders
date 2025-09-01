using System;
using System.Threading.Tasks;
using Client.Rede; // Certifique-se que o namespace está correto

class Program
{
    static async Task Main(string[] args)
    {
        var client = new TcpClientWrapper();
        var handleGame = new HandleGame(client);

        // Eventos
        client.OnMessageReceived += msg =>
        {
            Console.WriteLine($"Mensagem recebida do servidor: {msg}");
        };

        client.OnDisconnected += () =>
        {
            Console.WriteLine("Desconectado do servidor.");
        };

        try
        {
            Console.WriteLine("Conectando ao servidor...");
            await client.ConnectAsync("127.0.0.1", 9000); // IP e porta do servidor
            Console.WriteLine("Conectado!");

            while (true)
            {
                Console.Write("Digite uma mensagem para enviar (ou 'sair'): ");
                string? input = Console.ReadLine();

                if (input == "ok")
                {
                    await client.SendAsync(new
                    {
                        type = "Input",
                        thrust = 1,
                        rotate = 0,
                        shoot = true,
                        dt = 0.016
                    });
                    continue;
                }

                if (input == null || input.ToLower() == "sair")
                {
                    await client.SendAsync(input);
                    break;
                }

                await client.SendAsync(input);
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Erro: {ex.Message}");
        }
        finally
        {
            client.Disconnect();
        }

        Console.WriteLine("Programa finalizado.");
    }
}
