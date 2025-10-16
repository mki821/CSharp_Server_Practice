namespace Server.Command
{
    interface ICommand<T>
    {
        public Task ExecuteAsync(User user, T packet);
    }
}
