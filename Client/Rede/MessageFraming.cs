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
        var json = JsonSerializer.SerializeToUtf8Bytes(msg);
        var len = BitConverter.GetBytes(json.Length);
        await stream.WriteAsync(len, 0, len.Length, ct);
        await stream.WriteAsync(json, 0, json.Length, ct);
        await stream.FlushAsync(ct);
    }

    public static async Task<dynamic?> ReadAsync(NetworkStream stream, CancellationToken ct)
    {

        byte[] lenBuf = new byte[4];
        int read = await stream.ReadAsync(lenBuf, 0, 4, ct);
        if (read == 0) return null; // desconexão

        int len = BitConverter.ToInt32(lenBuf, 0);

        byte[] jsonBuffer = new byte[len];
        int readJson = 0;
        while (readJson < len)
        {
            int r = await stream.ReadAsync(jsonBuffer, readJson, len - readJson, ct);
            if (r == 0) return null;
            readJson += r;
        }
        return JsonSerializer.Deserialize<dynamic>(jsonBuffer);
    }
}