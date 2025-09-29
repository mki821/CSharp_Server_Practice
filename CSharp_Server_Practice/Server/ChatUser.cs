using System.Net.Sockets;
using System.Text;

namespace Server
{
    class ChatUser
    {
        public string? Nickname { get; set; }
        public TcpClient Client { get; }
        public NetworkStream Stream => Client.GetStream();
        public ChatRoom? Room { get; set; }
        public HashSet<string> Friends { get; } = new HashSet<string>();

        public ChatUser(TcpClient client) => Client = client;

        public async Task SendAsync(string message)
        {
            byte[] buffer = Encoding.UTF8.GetBytes(message + '\n');
            await Stream.WriteAsync(buffer);
        }
    }
}
