using Mirror;
using TMPro;
using UnityEngine;

/// <summary>
/// Minimal chat UI that sends messages via PlayerNetwork.
/// </summary>
public class ChatUI : MonoBehaviour
{
    public TMP_InputField inputField;
    public TextMeshProUGUI outputArea;

    public static ChatUI Instance { get; private set; }

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    public void OnSend()
    {
        if (inputField == null) return;
        string text = inputField.text.Trim();
        if (string.IsNullOrWhiteSpace(text)) return;

        if (NetworkClient.localPlayer != null)
        {
            var player = NetworkClient.localPlayer.GetComponent<PlayerNetwork>();
            player?.CmdSendChat(new ChatMessage { text = text });
        }
        inputField.text = string.Empty;
    }

    public void Receive(ChatMessage message)
    {
        if (outputArea != null)
        {
            outputArea.text += message.text + "\n";
        }
    }
}
