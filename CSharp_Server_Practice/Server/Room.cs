using Server.Packet;

namespace Server
{
    class Room
    {
        public string Name { get; set; }
        private readonly HashSet<User> _users = new HashSet<User>();
        private readonly object _locker = new object();

        public Room(string name) => Name = name;

        public void Join(User user)
        {
            lock (_locker)
            {
                _users.Add(user);
                user.Room = this;
                user.State = UserState.InRoom;
            }

            BroadcastSystem($"{user.Nickname} 님이 {Name} 방에 참가하였습니다..");
        }

        public void Leave(User user)
        {
            lock (_locker)
            {
                _users.Remove(user);
                user.Room = null;
            }

            BroadcastSystem($"{user.Nickname} 님이 {Name} 방에서 나가셨습니다.");
        }

        public void Broadcast(object packet)
        {
            List<User> snapshot;
            lock (_locker)
            {
                snapshot = _users.ToList();
            }

            foreach(User user in snapshot)
            {
                _ = user.SendAsync(packet);
            }
        }

        public void BroadcastSystem(string message)
        {
            ChatPacket packet = new ChatPacket { Sender = "SERVER", Message = message };
            Broadcast(packet);
        }

        public bool TryFindUser(string nickname, out User? user)
        {
            lock (_locker)
            {
                user = _users.FirstOrDefault(user => user.Nickname == nickname);
                return user != null;
            }
        }
    }
}
