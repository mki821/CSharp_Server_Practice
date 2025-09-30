using Server.Packet;

namespace Server.Command
{
    class FriendRequestCommand : ICommand<FriendRequestPacket>
    {
        public async Task ExecuteAsync(User user, FriendRequestPacket packet)
        {
            Server.Instance.FriendRepository.SendFriendRequest(packet.From, packet.To);

            if (Server.Instance.TryGetUserByNick(packet.To, out var target))
            {
                var notify = new ChatPacket { Sender = "SERVER", Message = $"{packet.From} 님이 친구 요청을 보냈습니다." };

                await target.SendAsync(notify);
                await user.SendAsync(new ServerMessagePacket { Message = "FRIEND_REQUEST_SENT_ONLINE" });
            }
            else
            {
                await user.SendAsync(new ServerMessagePacket { Message = "FRIEND_REQUEST_SENT_OFFLINE" });
            }
        }
    }
}
