using System.Buffers;
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
            byte[] buffer = new byte[4];

            int got = await socket.ReceiveAsync(buffer.AsMemory(), SocketFlags.None);
            if (got == 0) throw new Exception("Connection Closed!");

            int length = BitConverter.ToInt32(buffer);
            if (length == 0) throw new Exception("Invalid Length!");

            int offset = 0;
            ArrayPool<byte> pool = ArrayPool<byte>.Shared;
            byte[] body = pool.Rent(length);

            try
            {
                while(offset < length)
                {
                    int receive = await socket.ReceiveAsync(body.AsMemory(offset, length - offset), SocketFlags.None);
                    if (receive == 0) throw new Exception("Connection Closed!");
                    offset += receive;
                }

                var exact = new byte[length];
                Buffer.BlockCopy(body, 0, exact, 0, length);

                return exact;
            }
            finally
            {
                pool.Return(body, true);
            }
        }
    }
}
