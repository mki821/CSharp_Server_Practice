using Server.Packet;

namespace Server.Command
{
    class JoinCommand : ICommand<JoinPacket>
    {
        public async Task ExecuteAsync(User user, JoinPacket packet)
        {
            var room = Server.Instance.GetOrCreateRoom(packet.RoomName);

            user.Room?.Leave(user);
            room.Join(user);

            await user.SendAsync(new ServerMessagePacket { Message = $"JOINED:{packet.RoomName}" });
        }
    }
}
