using MessagePack;

namespace Server.Command
{
    class CommandDispatcher
    {
        private readonly Dictionary<string, Func<User, byte[], Task>> _handlers = new Dictionary<string, Func<User, byte[], Task>>();

        public void Register<T>(string type, ICommand<T> command)
        {
            _handlers[type] = async (user, raw) =>
            {
                T packet = MessagePackSerializer.Deserialize<T>(raw);
                await command.ExecuteAsync(user, packet);
            };
        }

        public Task DispatchAsync(User user, byte[] rawData)
        {
            Dictionary<string, object> header = MessagePackSerializer.Deserialize<Dictionary<string, object>>(rawData);
            if (!header.TryGetValue("Type", out var tObj))
                return Task.CompletedTask;

            string type = tObj.ToString();

            if(_handlers.TryGetValue(type, out var handler))
            {
                return handler(user, rawData);
            }

            return Task.CompletedTask;
        }
    }
}
