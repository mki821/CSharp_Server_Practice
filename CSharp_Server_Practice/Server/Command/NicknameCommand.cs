using Server.Packet;

namespace Server.Command
{
    class NicknameCommand : ICommand<NicknamePacket>
    {
        public async Task ExecuteAsync(User user, NicknamePacket packet)
        {
            if(user.State != UserState.AwaitingNickname)
            {
                await user.SendAsync(new ServerMessagePacket { Message = "ALREADY_SET" });

                return;
            }

            if(!Server.Instance.TryRegisterNickname(user, packet.Nickname, out var reason))
            {
                await user.SendAsync(new ServerMessagePacket { Message = $"NICK_REJECT:{reason}" });
            }

            await user.SendAsync(new ServerMessagePacket { Message = "NICK_OK" });

            var lobby = Server.Instance.GetOrCreateRoom("로비");
            lobby.Join(user);
        }
    }
}
