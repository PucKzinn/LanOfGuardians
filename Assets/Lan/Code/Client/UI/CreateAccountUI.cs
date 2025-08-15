// Caminho: Assets/Lan/Code/Client/UI/CreateAccountUI.cs
using UnityEngine;
using Mirror;
using TMPro;
using UnityEngine.UI;

public class CreateAccountUI : MonoBehaviour
{
    [Header("Campos (TMP)")]
    public TMP_InputField userField;
    public TMP_InputField passField;
    public TMP_InputField confirmField;
    public TMP_InputField addressFieldTMP; // opcional

    [Header("Opções")]
    public Toggle hostModeToggle; // Host (dev)

    [Header("UI")]
    public TextMeshProUGUI statusTMP;
    public LoginScreenManager screenManager; // voltar para conectar

    [Header("Rede")]
    public NetworkManager manager;
    public SimpleAuthenticator authenticator;

    void Awake()
    {
        if (manager == null) manager = NetworkManager.singleton;
        ShowStatus("Crie sua conta.");
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

    public void OnClickCreateAndConnect()
    {
        if (manager == null) manager = NetworkManager.singleton;
        if (authenticator == null) { ShowStatus("Authenticator não atribuído no NetworkManager."); return; }

        string user = (userField ? userField.text.Trim() : "");
        string pass = (passField ? passField.text : "");
        string conf = (confirmField ? confirmField.text : "");

        if (string.IsNullOrWhiteSpace(user)) { ShowStatus("Usuário vazio."); return; }
        if (string.IsNullOrWhiteSpace(pass)) { ShowStatus("Senha vazia.");   return; }
        if (pass != conf) { ShowStatus("As senhas não coincidem."); return; }

        if (addressFieldTMP && !string.IsNullOrWhiteSpace(addressFieldTMP.text))
            manager.networkAddress = addressFieldTMP.text.Trim();

        // Criar se faltar (fluxo de autenticação cria e já conecta)
        authenticator.cachedUsername = user;
        authenticator.cachedPassword = pass;
        authenticator.cachedCreate   = true;

        bool host = hostModeToggle && hostModeToggle.isOn;
        if (host)
        {
            ShowStatus("Criando e iniciando Host…");
            manager.StartHost();
        }
        else
        {
            ShowStatus($"Criando e conectando em {manager.networkAddress}…");
            manager.StartClient();
        }
    }

    public void OnClickBackToConnect()
    {
        if (screenManager) screenManager.ShowConnect();
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
        if (serverMessage == "OK") return "Conta criada e login efetuado.";
        if (serverMessage.Contains("Credenciais inválidas")) return "Credenciais inválidas ou já existentes.";
        return serverMessage;
    }

    void ShowStatus(string s)
    {
        if (statusTMP) statusTMP.text = s;
        Debug.Log("[CreateAccountUI] " + s);
    }
}
