using Server.Packet;

namespace Server.Command
{
    class ChatCommand : ICommand<ChatPacket>
    {
        public Task ExecuteAsync(User user, ChatPacket packet)
        {
            if(user.State != UserState.InRoom || user.Room == null)
            {
                return user.SendAsync(new ServerMessagePacket { Message = "JOIN_ROOM_FIRST" });
            }

            var chat = new ChatPacket { Sender = user.Nickname, Message = packet.Message };

            user.Room.Broadcast(chat);
            return Task.CompletedTask;
        }
    }
}
