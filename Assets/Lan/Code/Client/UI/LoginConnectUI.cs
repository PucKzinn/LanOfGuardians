// Caminho: Assets/Lan/Code/Client/UI/LoginConnectUI.cs
using UnityEngine;
using Mirror;
using TMPro;
using UnityEngine.UI;

public class LoginConnectUI : MonoBehaviour
{
    [Header("Campos (TMP)")]
    public TMP_InputField userField;
    public TMP_InputField passField;
    public TMP_InputField addressFieldTMP; // opcional

    [Header("Opções")]
    public Toggle hostModeToggle; // Host (dev)

    [Header("UI")]
    public TextMeshProUGUI statusTMP;
    public LoginScreenManager screenManager; // para ir para "Criar Conta"

    [Header("Rede")]
    public NetworkManager manager;
    public SimpleAuthenticator authenticator;

    void Awake()
    {
        if (manager == null) manager = NetworkManager.singleton;
        ShowStatus("Pronto para conectar.");
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

    public void OnClickConnect()
    {
        if (manager == null) manager = NetworkManager.singleton;
        if (authenticator == null) { ShowStatus("Authenticator não atribuído no NetworkManager."); return; }

        string user = (userField ? userField.text.Trim() : "");
        string pass = (passField ? passField.text : "");

        if (string.IsNullOrWhiteSpace(user)) { ShowStatus("Usuário vazio."); return; }
        if (string.IsNullOrWhiteSpace(pass)) { ShowStatus("Senha vazia.");   return; }

        if (addressFieldTMP && !string.IsNullOrWhiteSpace(addressFieldTMP.text))
            manager.networkAddress = addressFieldTMP.text.Trim();

        // Conectar (sem criar conta)
        authenticator.cachedUsername = user;
        authenticator.cachedPassword = pass;
        authenticator.cachedCreate   = false;

        bool host = hostModeToggle && hostModeToggle.isOn;
        if (host)
        {
            ShowStatus("Iniciando Host…");
            manager.StartHost();
        }
        else
        {
            ShowStatus($"Conectando em {manager.networkAddress}…");
            manager.StartClient();
        }
    }

    public void OnClickGoCreate()
    {
        if (screenManager) screenManager.ShowCreate();
    }

    void OnAuthResponse(SimpleAuthenticator.AuthResponse msg)
    {
        ShowStatus(MapAuthMessage(msg.message));
    }
    void OnAuthFailed(string serverMessage)
    {
        ShowStatus(MapAuthMessage(serverMessage));
    }

    string MapAuthMessage(string serverMessage)
    {
        if (string.IsNullOrWhiteSpace(serverMessage)) return "Falha ao autenticar.";
        if (serverMessage == "OK") return "Login realizado com sucesso.";
        if (serverMessage.Contains("Credenciais inválidas")) return "Credenciais inválidas.";
        return serverMessage;
    }

    void ShowStatus(string s)
    {
        if (statusTMP) statusTMP.text = s;
        Debug.Log("[LoginConnectUI] " + s);
    }
}
