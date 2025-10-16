using System.Net;
using System.Net.Sockets;
using Server.Command;
using Server.DB;
using Server.Packet;

namespace Server
{
    class Server
    {
        public static Server Instance { get; private set; }

        private readonly Socket _listener;
        private readonly CommandDispatcher _dispatcher = new CommandDispatcher();
        private readonly Dictionary<string, User> _usersByNickname = new Dictionary<string, User>();
        private readonly Dictionary<Socket, User> _usersBySocket = new Dictionary<Socket, User>();
        private readonly Dictionary<string, Room> _rooms = new Dictionary<string, Room>();

        private readonly object _usersLocker = new object();
        private readonly object _roomsLocker = new object();

        public readonly UserRepository UserRepository;
        public readonly FriendRepository FriendRepository;

        public Server(int port)
        {
            Instance = this;

            _listener = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            _listener.Bind(new IPEndPoint(IPAddress.Any, port));
            _listener.Listen(100);

            UserRepository = new UserRepository("users.db");
            FriendRepository = new FriendRepository("users.db");

            _dispatcher.Register("nickname", new NicknameCommand());
            _dispatcher.Register("chat", new ChatCommand());
            _dispatcher.Register("whisper", new WhisperCommand());
            _dispatcher.Register("firend_request", new FriendRequestCommand());
            _dispatcher.Register("join", new JoinCommand());
        }

        public async Task StartAsync()
        {
            Console.WriteLine($"[서버] {((_listener.LocalEndPoint as IPEndPoint)?.Port ?? 0)} 번 포트에서 서버 시작됨!");

            while (true)
            {
                Socket client = await _listener.AcceptAsync();
                Console.WriteLine($"[서버] 클라이언트 연결됨 : {client.RemoteEndPoint}");
                _ = HandleClientAsync(client);
            }
        }

        private async Task HandleClientAsync(Socket socket)
        {
            User user = new User(socket);
            lock (_usersLocker)
            {
                _usersBySocket[socket] = user;
            }

            await PacketHelper.SendPacketAsync(socket, new ServerMessagePacket { Message = "SEND_NICKNAME" });

            try
            {
                while (true)
                {
                    byte[] raw = await PacketHelper.ReceiveRawAsync(socket);
                    await _dispatcher.DispatchAsync(user, raw);
                }
            }
            catch
            {
                Console.WriteLine($"[서버] 클라이언트 연결 해제됨 : {socket.RemoteEndPoint}");
            }
            finally
            {
                DisconnectUser(user);
            }
        }

        public void DisconnectUser(User user)
        {
            if (user == null) return;
            
            lock (_usersLocker)
            {
                if(!string.IsNullOrEmpty(user.Nickname) && _usersByNickname.ContainsKey(user.Nickname))
                {
                    _usersByNickname.Remove(user.Nickname);
                    UserRepository.Logout(user.Nickname);
                }
                if (_usersBySocket.ContainsKey(user.Socket))
                {
                    _usersBySocket.Remove(user.Socket);
                }
            }

            user.Room?.Leave(user);

            try { user.Socket.Shutdown(SocketShutdown.Both); } catch { }
            try { user.Socket.Close(); } catch { }

            Console.WriteLine($"[서버] {user.Nickname ?? "(unknown)"} 에 대한 처리 완료!");
        }

        public bool TryRegisterNickname(User user, string nickname, out string reason)
        {
            reason = string.Empty;

            if (string.IsNullOrEmpty(nickname))
            {
                reason = "empty";

                return false;
            }

            lock (_usersLocker)
            {
                if (_usersByNickname.ContainsKey(nickname))
                {
                    reason = "already_online";

                    return false;
                }

                bool ok = UserRepository.TryLogin(nickname);
                if(!ok)
                {
                    reason = "db_online";

                    return false;
                }

                user.Nickname = nickname;
                _usersByNickname[nickname] = user;

                return true;
            }
        }

        public bool TryGetUserByNick(string nickname, out User user)
        {
            lock (_usersLocker)
            {
                return _usersByNickname.TryGetValue(nickname, out user);
            }
        }

        public Room GetOrCreateRoom(string name)
        {
            lock (_roomsLocker)
            {
                if(!_rooms.TryGetValue(name, out Room? room))
                {
                    room = new Room(name);
                    _rooms[name] = room;
                }

                return room;
            }
        }
    }
}
