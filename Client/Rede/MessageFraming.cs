using System; 
using System.IO; 
using System.Text; 
using System.Text.Json;
using System.Threading; 
using System.Threading.Tasks; 
using System.Net.Sockets;
using System.Buffers.Binary;
using System.Text;
using System.Text.Json;

namespace Client.Rede
{
    public static class MessageFraming
    {
        public static async Task SendMessageAsync(NetworkStream stream, object msg, CancellationToken ct)
        {
            if (stream == null) throw new ArgumentNullException(nameof(stream));

            try
            {
                byte[] json = JsonSerializer.SerializeToUtf8Bytes(msg);
                byte[] len = new byte[4];
                BinaryPrimitives.WriteInt32LittleEndian(len, json.Length);

                await stream.WriteAsync(len, ct);
                await stream.WriteAsync(json, ct);
                await stream.FlushAsync(ct);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[MessageFraming] Erro ao enviar mensagem: {ex.Message}");
                throw;
            }
        }

        public static async Task<JsonElement?> ReadAsync(NetworkStream stream, CancellationToken ct)
        {
            if (stream == null) throw new ArgumentNullException(nameof(stream));

            try
            {
                byte[] lenBuf = new byte[4];
                int read = await stream.ReadAsync(lenBuf.AsMemory(0, 4), ct);
                if (read == 0) return null;

                int len = BinaryPrimitives.ReadInt32LittleEndian(lenBuf);
                if (len <= 0 || len > 100_000) 
                {
                    Console.WriteLine($"[MessageFraming] Tamanho de mensagem inválido: {len}");
                    return null;
                }

                byte[] jsonBuffer = new byte[len];
                int readJson = 0;
                while (readJson < len)
                {
                    int r = await stream.ReadAsync(jsonBuffer.AsMemory(readJson, len - readJson), ct);
                    if (r == 0) return null;
                    readJson += r;
                }

                using var document = JsonDocument.Parse(jsonBuffer);
                return document.RootElement.Clone();
            }
            catch (JsonException ex)
            {
                Console.WriteLine($"[MessageFraming] Erro de JSON: {ex.Message}");
                return null;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[MessageFraming] Erro ao ler mensagem: {ex.Message}");
                return null;
            }
        }
    }
}
