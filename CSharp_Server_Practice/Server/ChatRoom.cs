namespace Server
{
    class ChatRoom
    {
        public string Name { get; }
        private readonly List<ChatUser> _users = new List<ChatUser>();
        private readonly object _locker = new object();

        public ChatRoom(string name) => Name = name;

        public void Join(ChatUser user)
        {
            lock(_locker)
            {
                _users.Add(user);
                user.Room = this;
            }

            Broadcast($"{user.Nickname} 님이 입장했습니다.");
        }

        public void Leave(ChatUser user)
        {
            lock(_locker)
            {
                _users.Remove(user);
                user.Room = null;
            }

            Broadcast($"{user.Nickname} 님이 퇴장했습니다.");
        }

        public void Broadcast(string message, ChatUser? except = null)
        {
            List<ChatUser> users;
            lock (_locker) users = [.._users];

            foreach(ChatUser user in users)
            {
                if (user == except) continue;
                _ = user.SendAsync(message);
            }
        }
    }
}
