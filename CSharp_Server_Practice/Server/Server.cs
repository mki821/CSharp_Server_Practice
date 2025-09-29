using System.Net;
using System.Net.Sockets;
using System.Text;

namespace Server
{
    class Server
    {
        private readonly TcpListener _listener;
        private readonly Dictionary<string, ChatUser> _usersByNickname = new Dictionary<string, ChatUser>();
        private readonly Dictionary<string, ChatRoom> _rooms = new Dictionary<string, ChatRoom>();

        public Server(int port)
        {
            _listener = new TcpListener(IPAddress.Any, port);
        }

        public async Task StartAsync()
        {
            _listener.Start();
            Console.WriteLine("[서버] 서버 열림!");

            while(true)
            {
                TcpClient client = await _listener.AcceptTcpClientAsync();
                _ = HandleClientAsync(new ChatUser(client));
            }
        }

        private async Task HandleClientAsync(ChatUser user)
        {
            Console.WriteLine($"[서버] 새로운 클라이언트가 접속함!");
            NetworkStream stream = user.Stream;
            byte[] buffer = new byte[1024];
            StringBuilder builder = new StringBuilder();

            try
            {
                while(true)
                {
                    int read = await stream.ReadAsync(buffer);
                    if (read == 0) break;

                    builder.Append(Encoding.UTF8.GetString(buffer, 0, read));

                    while (builder.ToString().Contains('\n'))
                    {
                        string text = builder.ToString();
                        int index = text.IndexOf('\n');
                        string line = text[..index].Trim();
                        builder.Remove(0, index + 1);

                        if (!string.IsNullOrEmpty(line))
                            await HandleCommand(user, line);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[서버] 에러남! | {ex.Message}");
            }
            finally
            {
                if (user.Room != null) user.Room.Leave(user);
                if (user.Nickname != null) _usersByNickname.Remove(user.Nickname);

                stream.Close();
                user.Client.Close();

                Console.WriteLine("[서버] 클라이언트가 접속 종료됨!");
            }
        }

        private async Task HandleCommand(ChatUser user, string line)
        {
            if(line.StartsWith("/이름 "))
            {
                string nick = line[4..].Trim();
                if(_usersByNickname.ContainsKey(nick))
                {
                    await user.SendAsync("닉네임이 중복됩니다!");
                }
                else
                {
                    user.Nickname = nick;
                    _usersByNickname[nick] = user;
                    await user.SendAsync($"닉네임이 {nick} 으로 설정되었습니다!");
                }
            }
            else if(line.StartsWith("/접속"))
            {
                string name = line[4..].Trim();
                if(!_rooms.TryGetValue(name, out var room))
                {
                    room = new ChatRoom(name);
                    _rooms[name] = room;
                }

                room.Join(user);
            }
            else if (line.StartsWith("/귓"))
            {
                string[] parts = line.Split(' ');
                if (parts.Length < 3) return;
                string nick = parts[1];
                string message = parts[2];
                if(_usersByNickname.TryGetValue(nick, out var target))
                {
                    await target.SendAsync($"[{user.Nickname} 님으로부터 온 귓속말] {message}");
                    await user.SendAsync($"[{nick} 님에게 보낸 귓속말] {message}");
                }
            }
            else if (line.StartsWith("/친구"))
            {
                string[] parts = line.Split(' ');
                if (parts.Length < 2) return;

                if (parts[1] == "추가" && parts.Length == 3)
                {
                    user.Friends.Add(parts[2]);
                    await user.SendAsync($"{parts[2]} 님을 팔로우 중입니다!");
                }
                else if (parts[1] == "목록")
                {
                    await user.SendAsync($"친구 목록 : {string.Join(", ", user.Friends)}");
                }
                else if (parts[1] == "삭제" && parts.Length == 3)
                {
                    user.Friends.Remove(parts[2]);
                    await user.SendAsync($"{parts[2]} 님을 팔로우 해제했습니다!");
                }
            }
            else
            {
                if(user.Room != null)
                {
                    user.Room.Broadcast($"{user.Nickname} : {line}", user);
                }
                else
                {
                    await user.SendAsync("먼저 \"/접속 [방이름]\" 으로 방에 들어가주세요!");
                }
            }
        }
    }
}
