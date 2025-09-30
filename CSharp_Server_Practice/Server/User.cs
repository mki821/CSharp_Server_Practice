using System.Net.Sockets;
using Server.Packet;

namespace Server
{
    public enum UserState
    {
        AwaitingNickname,
        InRoom
    }

    class User
    {
        public Socket Socket { get; }
        public string? Nickname { get; set; }
        public Room? Room { get; set; }
        public UserState State { get; set; } = UserState.AwaitingNickname;

        public User(Socket socket) => Socket = socket;

        public async Task SendAsync<T>(T packet)
        {
            try
            {
                await PacketHelper.SendPacketAsync(Socket, packet);
            }
            catch { }
        }
    }
}
