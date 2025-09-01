using System;
using System.IO;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using System.Net.Sockets;

namespace Client.Rede;
public static class MessageFraming
{
    public static async Task SendMessageAsync(NetworkStream stream, object msg, CancellationToken ct)
    {
        try
        {
            var json = JsonSerializer.SerializeToUtf8Bytes(msg);
            var len = BitConverter.GetBytes(json.Length);
            await stream.WriteAsync(len, 0, len.Length, ct);
            await stream.WriteAsync(json, 0, json.Length, ct);
            await stream.FlushAsync(ct);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Erro ao enviar mensagem: {ex.Message}");
            throw;
        }
    }

    public static async Task<JsonElement?> ReadAsync(NetworkStream stream, CancellationToken ct)
    {
        try
        {
            byte[] lenBuf = new byte[4];
            int read = await stream.ReadAsync(lenBuf, 0, 4, ct);
            if (read == 0) return null;

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
                int r = await stream.ReadAsync(jsonBuffer, readJson, len - readJson, ct);
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
            Console.WriteLine($"Erro ao ler mensagem: {ex.Message}");
            return null;
        }
    }
}