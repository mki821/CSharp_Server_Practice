using System.Net.Sockets;
using MessagePack;

namespace Server.Packet
{
    class PacketHelper
    {
        public static async Task SendPacketAsync<T>(Socket socket, T packet)
        {
            byte[] body = MessagePackSerializer.Serialize(packet);
            byte[] lengthPrefix = BitConverter.GetBytes(body.Length);

            await socket.SendAsync(lengthPrefix.AsMemory(), SocketFlags.None);
            await socket.SendAsync(body.AsMemory(), SocketFlags.None);
        }

        public static async Task<byte[]> ReceiveRawAsync(Socket socket)
        {
            byte[] lengthBytes = new byte[4];
            int raw = await socket.ReceiveAsync(lengthBytes.AsMemory(), SocketFlags.None);
            if (raw == 0) throw new Exception("Connection Closed!");

            int length = BitConverter.ToInt32(lengthBytes, 0);
            if (length <= 0) throw new Exception("Invalid Length!");

            byte[] body = new byte[length];
            int offset = 0;
            while(offset < length)
            {
                int got = await socket.ReceiveAsync(body.AsMemory(offset, length - offset), SocketFlags.None);
                if (got == 0) throw new Exception("Connection Closed!");
                offset += got;
            }

            return body;
        }
    }
}
