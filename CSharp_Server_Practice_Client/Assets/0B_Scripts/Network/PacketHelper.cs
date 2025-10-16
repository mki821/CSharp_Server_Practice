using System;
using System.Net.Sockets;
using System.Threading.Tasks;
using UnityEngine;
using MessagePack;
using MessagePack.Resolvers;
using MessagePack.Formatters;

public class PacketHelper : MonoBehaviour
{
    private static bool _initialized = false;

    public static void Initialize()
    {
        if (_initialized) return;

        var resolver = CompositeResolver.Create(new IMessagePackFormatter[] { },
            new IFormatterResolver[] { GeneratedResolver.Instance, StandardResolver.Instance });

        MessagePackSerializer.DefaultOptions = MessagePackSerializerOptions.Standard.WithResolver(resolver);

        _initialized = true;
    }

    public static async Task SendPacketAsync<T>(Socket socket, T packet)
    {
        if (!_initialized) Initialize();
        
        byte[] body = MessagePackSerializer.Serialize(packet);
        byte[] length = BitConverter.GetBytes(body.Length);
        await socket.SendAsync(length, SocketFlags.None);
        await socket.SendAsync(body, SocketFlags.None);
    }

    public static async Task<byte[]> ReceiveRawAsync(Socket socket)
    {
        if (!_initialized) Initialize();

        byte[] lengthBuffer = new byte[4];

        int got = await socket.ReceiveAsync(lengthBuffer, SocketFlags.None);
        if (got == 0) throw new Exception("Connection Closed!");

        int length = BitConverter.ToInt32(lengthBuffer, 0);
        if (length == 0) throw new Exception("Invalid Length!");

        byte[] buffer = new byte[length];
        int offset = 0;
        while (offset < length)
        {
            int receive = await socket.ReceiveAsync(buffer.AsMemory(offset, length - offset), SocketFlags.None);
            if (receive == 0) throw new Exception("Connection Closed!");
            offset += receive;
        }

        return buffer;
    }
}
