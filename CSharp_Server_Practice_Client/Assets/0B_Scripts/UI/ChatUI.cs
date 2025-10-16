using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ChatUI : MonoBehaviour
{
    [SerializeField] private TMP_InputField _nicknameInput;
    [SerializeField] private TMP_InputField _messageInput;
    [SerializeField] private TextMeshProUGUI _chatText;
    [SerializeField] private Button _connectButton;
    [SerializeField] private Button _sendButton;

    private void Start()
    {
        _connectButton.onClick.AddListener(() => Connect());
        _sendButton.onClick.AddListener(async () => await Send());

        NetworkClient.Instance.OnChatReceived += OnChat;
        NetworkClient.Instance.OnServerMessage += OnServerMessage;
    }

    private async void Connect()
    {
        await NetworkClient.Instance.ConnectAsync("127.0.0.1", 5050);
        await NetworkClient.Instance.SendNickname(_nicknameInput.text);
    }

    private async Task Send()
    {
        if (!NetworkClient.Instance.IsConnected) return;

        await NetworkClient.Instance.SendChat(_messageInput.text);
        _messageInput.text = "";
    }

    private void OnChat(ChatPacket packet)
    {
        _chatText.text += $"\n[{packet.Sender}] {packet.Message}";
    }

    private void OnServerMessage(string message)
    {
        _chatText.text += $"\n<color=yellow>[SERVER]</color> {message}";
    }
}
