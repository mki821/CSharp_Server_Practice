using System.Net;

namespace Server
{
    internal class Program
    {
        public async static Task Main()
        {
            Server server = new Server(5050);
            await server.StartAsync();
        }
    }
}
