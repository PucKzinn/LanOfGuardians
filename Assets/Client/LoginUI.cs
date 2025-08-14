using UnityEngine;
using UnityEngine.UI;
using Mirror;

// Se usar TextMeshPro, descomente a linha abaixo e troque os campos no Inspector
using TMPro;

public class LoginUI : MonoBehaviour
{
    [Header("Campos de Login")]
    // Use EITHER InputField OR TMP_InputField (comente um ou outro)
    // public InputField userField;
    // public InputField passField;
    public TMP_InputField userFieldTMP;
    public TMP_InputField passFieldTMP;

    [Header("Opções")]
    public Toggle createIfMissingToggle;
    public InputField addressField;           // opcional (IP/Host), deixe vazio para usar o padrão do NetworkManager
    public Text statusText;                   // opcional (mensagens de status na UI)

    [Header("Referências")]
    public NetworkManager manager;            // arraste o seu NetworkManager (PlayerSpawner)
    public SimpleAuthenticator authenticator; // arraste o SimpleAuthenticator do objeto NetworkManager
           
    float lastClickTime;    // anti-spam básico

    void Awake()
    {
        if (manager == null) manager = NetworkManager.singleton;
        ShowStatus("Pronto.");
    }

    void OnEnable() => SimpleAuthenticator.AuthResponseReceived += OnAuthResponse;
    void OnDisable() => SimpleAuthenticator.AuthResponseReceived -= OnAuthResponse;

    void OnEnable()
    {
        SimpleAuthenticator.ClientAuthFailed += OnAuthFailed;
    }

    void OnDisable()
    {
        SimpleAuthenticator.ClientAuthFailed -= OnAuthFailed;
    }


    // dentro de LoginUI.cs
    public void Connect()
    {
        if (manager == null) manager = NetworkManager.singleton;

        string username = GetUser();
        string password = GetPass();
        bool createIfMissing = createIfMissingToggle != null && createIfMissingToggle.isOn;

        if (string.IsNullOrWhiteSpace(username)) { ShowStatus("Usuário vazio."); return; }
        if (string.IsNullOrWhiteSpace(password)) { ShowStatus("Senha vazia."); return; }

        if (addressField != null && !string.IsNullOrWhiteSpace(addressField.text))
            manager.networkAddress = addressField.text.Trim();

        // passa as credenciais para o AUTHENTICATOR
        authenticator.cachedUsername = username;
        authenticator.cachedPassword = password;
        authenticator.cachedCreate   = createIfMissing;

        ShowStatus($"Conectando em {manager.networkAddress}...");
        // o SimpleAuthenticator vai enviar LoginMessage no OnClientAuthenticate()
        manager.StartClient(); // (em vez de NetworkClient.Connect + OnConnectedEvent)
    }


    // -------- envio do login --------
    string pendingUsername, pendingPassword;
    bool pendingCreate;

    void OnClientConnectedSendLogin()
    {
        if (authenticator == null)
        {
            ShowStatus("Authenticator não atribuído no NetworkManager.");
            return;
        }

        var msg = new SimpleAuthenticator.LoginMessage
        {
            username = pendingUsername,
            password = pendingPassword,
            createIfMissing = pendingCreate
        };

        NetworkClient.Send(msg);
        ShowStatus("Login enviado.");
    }

    void OnClientDisconnected()
    {
        ShowStatus("Desconectado do servidor.");
    }

    void OnAuthFailed(string serverMessage)
    {
        ShowStatus(MapAuthMessage(serverMessage));
    }

    string MapAuthMessage(string msg)
    {
        if (string.IsNullOrWhiteSpace(msg)) return "Falha ao autenticar.";
        if (msg.Contains("Credenciais inválidas")) return "Credenciais inválidas.";
        return msg;
    }

    // -------- UI helpers --------
    string GetUser()
    {
        // se usar TMP, troque para userFieldTMP.text
       return userFieldTMP != null ? userFieldTMP.text.Trim() : "";  /* : userFieldTMP.text.Trim(); */
    }

    string GetPass()
    {
        // se usar TMP, troque para passFieldTMP.text
        return passFieldTMP != null ? passFieldTMP.text.Trim() : ""; /* : passFieldTMP.text */
    }

    void ShowStatus(string msg)
    {
        if (statusText != null) statusText.text = msg;
        // também manda pro console
        Debug.Log($"[LoginUI] {msg}");
    }

    void OnAuthResponse(SimpleAuthenticator.AuthResponse msg) =>
        ShowStatus(MapAuthMessage(msg.message));

    static string MapAuthMessage(string serverMessage) => serverMessage switch
    {
        "Credenciais inválidas." => "Credenciais inválidas",
        "OK" => "Login realizado com sucesso.",
        _ => serverMessage
    };
}
