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

    public static async Task<string> ReadAsync(NetworkStream stream, CancellationToken ct)
    {
        byte[] jsonBuffer = new byte[32]; // folha em branco com tamanho exato

       var len =  await stream.ReadAsync(jsonBuffer, 0, jsonBuffer.Length, ct);

        return Encoding.ASCII.GetString(jsonBuffer, 0, len);
    }
}