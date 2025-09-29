using System.Net.Sockets;
using System.Text;

namespace Client
{
    internal class Program
    {
        public async static Task Main()
        {
            Console.Write("[클라이언트] 접속하려는 서버 ip를 입력해주세요 : ");
            string ip = Console.ReadLine() ?? "127.0.0.1";

            TcpClient client = new TcpClient();
            await client.ConnectAsync(ip, 5050);
            Console.WriteLine("[클라이언트] 서버에 접속했습니다!");

            NetworkStream stream = client.GetStream();

            _ = Task.Run(async () =>
            {
                byte[] buffer = new byte[1024];
                while(true)
                {
                    int read = await stream.ReadAsync(buffer);
                    if (read == 0) break;

                    string message = Encoding.UTF8.GetString(buffer, 0, read);
                    Console.WriteLine(message.Trim());
                }
            });

            while(true)
            {
                string? line = Console.ReadLine();
                if (string.IsNullOrEmpty(line)) continue;

                byte[] buffer = Encoding.UTF8.GetBytes(line + '\n');
                await stream.WriteAsync(buffer);
            }
        }
    }
}
