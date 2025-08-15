// Caminho: Assets/Lan/Code/Client/UI/LoginUI.cs
using UnityEngine;
using UnityEngine.UI;
using Mirror;
using TMPro;

public class LoginUI : MonoBehaviour
{
    [Header("Campos de Login (TMP)")]
    public TMP_InputField userFieldTMP;
    public TMP_InputField passFieldTMP;

    [Header("Opções")]
    public Toggle createIfMissingToggle;
    public Toggle hostModeToggle;     // ⇦ NOVO
    public InputField addressField;   // opcional (IP/Host)
    public Text statusText;           // opcional (UI de status)

    [Header("Referências")]
    public NetworkManager manager;
    public SimpleAuthenticator authenticator;

    void Awake()
    {
        if (manager == null) manager = NetworkManager.singleton;
        ShowStatus("Pronto.");
    }

    void OnEnable()
    {
        SimpleAuthenticator.AuthResponseReceived += OnAuthResponse;
        SimpleAuthenticator.ClientAuthFailed     += OnAuthFailed;
    }
    void OnDisable()
    {
        SimpleAuthenticator.AuthResponseReceived -= OnAuthResponse;
        SimpleAuthenticator.ClientAuthFailed     -= OnAuthFailed;
    }

    public void Connect()
    {
        if (manager == null) manager = NetworkManager.singleton;
        if (authenticator == null) { ShowStatus("Authenticator não atribuído no NetworkManager."); return; }

        string username = GetUser();
        string password = GetPass();
        bool createIfMissing = createIfMissingToggle != null && createIfMissingToggle.isOn;
        
        if (string.IsNullOrWhiteSpace(username)) { ShowStatus("Usuário vazio."); return; }
        if (string.IsNullOrWhiteSpace(password)) { ShowStatus("Senha vazia.");   return; }

        if (addressField != null && !string.IsNullOrWhiteSpace(addressField.text))
            manager.networkAddress = addressField.text.Trim();

        // credenciais ANTES de iniciar
        authenticator.cachedUsername = username;
        authenticator.cachedPassword = password;
        authenticator.cachedCreate   = createIfMissing;
        authenticator.cachedCreate = true;

        bool host = hostModeToggle != null && hostModeToggle.isOn;
        if (host)
        {
            ShowStatus("Iniciando Host (dev)...");
            manager.StartHost();
        }
        else
        {
            ShowStatus($"Conectando em {manager.networkAddress}...");
            manager.StartClient();
        }
    }

    void OnAuthResponse(SimpleAuthenticator.AuthResponse msg) => ShowStatus(MapAuthMessage(msg.message));
    void OnAuthFailed(string serverMessage) => ShowStatus(MapAuthMessage(serverMessage));

    string GetUser() => userFieldTMP != null ? userFieldTMP.text.Trim() : "";
    string GetPass() => passFieldTMP != null ? passFieldTMP.text        : "";

    void ShowStatus(string msg) { if (statusText != null) statusText.text = msg; Debug.Log($"[LoginUI] {msg}"); }
    string MapAuthMessage(string serverMessage)
    {
        if (string.IsNullOrWhiteSpace(serverMessage)) return "Falha ao autenticar.";
        if (serverMessage == "OK") return "Login realizado com sucesso.";
        if (serverMessage.Contains("Credenciais inválidas")) return "Credenciais inválidas.";
        return serverMessage;
    }
}
