using Server.Packet;

namespace Server.Command
{
    class WhisperCommand : ICommand<WhisperPacket>
    {
        public async Task ExecuteAsync(User user, WhisperPacket packet)
        {
            if(!Server.Instance.TryGetUserByNick(packet.To, out var target))
            {
                await user.SendAsync(new ServerMessagePacket { Message = "USER_NOT_FOUND" });

                return;
            }

            var chat = new ChatPacket { Sender = $"(whisper) {user.Nickname}", Message = packet.Message };

            await target.SendAsync(chat);
            await user.SendAsync(new ServerMessagePacket { Message = "WHISPER_SENT" });
        }
    }
}
