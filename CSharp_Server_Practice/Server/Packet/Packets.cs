using MessagePack;

namespace Server.Packet
{
    [MessagePackObject]
    public class ChatPacket
    {
        [Key(0)] public string Type { get; set; } = "chat";
        [Key(1)] public string Sender { get; set; }
        [Key(2)] public string Message { get; set; }
    }

    [MessagePackObject]
    public class FriendRequestPacket
    {
        [Key(0)] public string Type { get; set; } = "friend_request";
        [Key(1)] public string From { get; set; }
        [Key(2)] public string To { get; set; }
    }

    [MessagePackObject]
    public class NicknamePacket
    {
        [Key(0)] public string Type { get; set; } = "nickname";
        [Key(1)] public string Nickname { get; set; }
    }

    [MessagePackObject]
    public class WhisperPacket
    {
        [Key(0)] public string Type { get; set; } = "whisper";
        [Key(1)] public string From { get; set; }
        [Key(2)] public string To { get; set; }
        [Key(3)] public string Message { get; set; }
    }

    [MessagePackObject]
    public class JoinPacket
    {
        [Key(0)] public string Type { get; set; } = "join";
        [Key(1)] public string RoomName { get; set; }
    }

    [MessagePackObject]
    public class ServerMessagePacket
    {
        [Key(0)] public string Type { get; set; } = "server_msg";
        [Key(1)] public string Message { get; set; }
    }
}
