namespace Server.Command
{
    interface ICommand<T>
    {
        Task ExecuteAsync(User user, T packet);
    }
}
