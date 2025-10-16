using System;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using System.Buffers;
using UnityEngine;
using MessagePack;

public class NetworkClient : MonoBehaviour
{
    public static NetworkClient Instance { get; private set; }

    private Socket _socket;
    private CancellationTokenSource _cts;

    public bool IsConnected => _socket?.Connected ?? false;

    public event Action<ChatPacket> OnChatReceived;
    public event Action<string> OnServerMessage;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public async Task ConnectAsync(string ip, int port)
    {
        _socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        await _socket.ConnectAsync(ip, port);

        Debug.Log("[클라이언트] 서버에 연결됨!");

        _cts = new CancellationTokenSource();
        _ = Task.Run(() => ListenAsync(_cts.Token));
    }

    private async Task ListenAsync(CancellationToken token)
    {
        try
        {
            while (!token.IsCancellationRequested)
            {
                byte[] raw = await PacketHelper.ReceiveRawAsync(_socket);
                if (raw == null || raw.Length == 0) continue;

                var reader = new MessagePackReader(new ReadOnlySequence<byte>(raw));
                int count = reader.ReadArrayHeader();
                if (count <= 0) continue;

                string type = reader.ReadString();

                switch (type)
                {
                    case "chat":
                        ChatPacket chat = MessagePackSerializer.Deserialize<ChatPacket>(raw);
                        OnChatReceived?.Invoke(chat);
                        break;
                    case "server_msg":
                        ServerMessagePacket serverMessage = MessagePackSerializer.Deserialize<ServerMessagePacket>(raw);
                        OnServerMessage?.Invoke(serverMessage.Message);
                        break;
                }
            }
        }
        catch (Exception ex)
        {
            Debug.LogWarning($"[클라이언트] 연결 해제됨! ({ex.Message})");
        }
    }

    public Task SendNickname(string nickname)
    {
        NicknamePacket packet = new NicknamePacket { Nickname = nickname };
        return PacketHelper.SendPacketAsync<NicknamePacket>(_socket, packet);
    }

    public Task SendChat(string message)
    {
        ChatPacket packet = new ChatPacket { Sender = "Me", Message = message };
        return PacketHelper.SendPacketAsync<ChatPacket>(_socket, packet);
    }
    
    private void OnApplicationQuit()
    {
        _cts?.Cancel();

        try
        {
            _socket?.Shutdown(SocketShutdown.Both);
        }
        catch { }

        _socket.Close();
    }
}
