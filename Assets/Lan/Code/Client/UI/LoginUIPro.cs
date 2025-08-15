// Assets/Lan/Code/Client/UI/LoginUIPro.cs
using System;
using System.Globalization;
using UnityEngine;
using UnityEngine.UI;
using Mirror;
using TMPro;

/// <summary>
/// Login UI completo com TMP, estados visuais, validação, "lembrar usuário",
/// mostrar/ocultar senha, spinner e integração com SimpleAuthenticator.
/// </summary>
public class LoginUIPro : MonoBehaviour
{
    [Header("Referências de Rede")]
    public NetworkManager manager;            // arraste o NetworkManager (PlayerSpawner)
    public SimpleAuthenticator authenticator; // arraste o SimpleAuthenticator usado pelo NetworkManager

    [Header("Campos (TMP)")]
    public TMP_InputField userField;
    public TMP_InputField passField;

    [Header("Controles")]
    public Toggle toggleCreateIfMissing;
    public Toggle toggleRememberUser;
    public Toggle toggleShowPassword;
    public Button  buttonLogin;
    public Button  buttonCancel; // opcional: cancela conexão
    public TMP_Text statusText;
    public TMP_Text titleText; // opcional

    [Header("Grupos de UI")]
    public CanvasGroup formGroup;       // painel do formulário
    public CanvasGroup connectingGroup; // painel "conectando..." com spinner

    [Header("Misc")]
    public InputField addressField; // se quiser IP/Host
    public RectTransform spinner;   // para rodar com RotateSpinner (opcional)

    const string PREF_REMEMBER = "login_remember_user";
    const string PREF_LASTUSER = "login_last_user";

    void Reset()
    {
        // Tentativa de auto-resolver referências no Reset do Inspector
        if (manager == null) manager = NetworkManager.singleton;
        if (authenticator == null && manager != null)
            authenticator = manager.authenticator as SimpleAuthenticator;
    }

    void Awake()
    {
        if (manager == null) manager = NetworkManager.singleton;
        if (authenticator == null && manager != null)
            authenticator = manager.authenticator as SimpleAuthenticator;

        // Carrega preferências
        bool remember = PlayerPrefs.GetInt(PREF_REMEMBER, 1) == 1;
        if (toggleRememberUser) toggleRememberUser.isOn = remember;
        if (remember)
        {
            string last = PlayerPrefs.GetString(PREF_LASTUSER, "");
            if (userField) userField.text = last;
        }

        // Estado inicial
        SetStateIdle();

        // Mostrar senha toggle
        if (toggleShowPassword)
            toggleShowPassword.onValueChanged.AddListener(OnToggleShowPassword);

        // Enter para enviar do campo senha
        if (passField)
            passField.onSubmit.AddListener(_ => OnClickLogin());

        // Enter no user envia foco para senha
        if (userField)
            userField.onSubmit.AddListener(_ => {
                if (passField) passField.Select();
            });

        // Botão cancelar (opcional)
        if (buttonCancel)
            buttonCancel.onClick.AddListener(OnClickCancel);
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

    // ============ UI Actions ============
    public void OnClickLogin()
    {
        if (manager == null) manager = NetworkManager.singleton;
        if (authenticator == null)
        {
            ShowStatus("Authenticator não atribuído no NetworkManager.");
            return;
        }

        string username = (userField ? userField.text.Trim() : "");
        string password = (passField ? passField.text : "");

        if (string.IsNullOrWhiteSpace(username)) { ShowStatus("Usuário vazio."); return; }
        if (string.IsNullOrWhiteSpace(password)) { ShowStatus("Senha vazia.");   return; }

        bool createIfMissing = toggleCreateIfMissing && toggleCreateIfMissing.isOn;

        // Remember user
        bool remember = toggleRememberUser ? toggleRememberUser.isOn : true;
        PlayerPrefs.SetInt(PREF_REMEMBER, remember ? 1 : 0);
        if (remember) PlayerPrefs.SetString(PREF_LASTUSER, username);

        // Endereço
        if (addressField && !string.IsNullOrWhiteSpace(addressField.text))
            manager.networkAddress = addressField.text.Trim();

        // Passa credenciais
        authenticator.cachedUsername = username;
        authenticator.cachedPassword = password;
        authenticator.cachedCreate   = createIfMissing;

        // UI -> Conectando
        SetStateConnecting($"Conectando em {manager.networkAddress}...");
        manager.StartClient();
    }

    public void OnClickCancel()
    {
        if (NetworkClient.isConnected || NetworkClient.active)
            manager.StopClient();
        SetStateIdle("Conexão cancelada.");
    }

    void OnToggleShowPassword(bool show)
    {
        if (!passField) return;
        passField.contentType = show ? TMP_InputField.ContentType.Standard : TMP_InputField.ContentType.Password;
        passField.ForceLabelUpdate();
    }

    // ============ Auth Callbacks ============
    void OnAuthResponse(SimpleAuthenticator.AuthResponse msg)
    {
        // Se sua AuthResponse tiver campos extras (ex.: code, lockedUntilUtc), trate aqui.
        // Este bloco funciona tanto para a resposta simples quanto para a estendida.
        // Caso esteja usando enum AuthCode e campos adicionais, descomente e ajuste:

        // if (msg.success)
        // {
        //     SetStateSuccess("Login realizado com sucesso.");
        //     return;
        // }
        //
        // // Exemplo de bloqueio temporário com horário local:
        // if (msg.code == (int)AuthCode.Locked && !string.IsNullOrEmpty(msg.lockedUntilUtc))
        // {
        //     if (DateTime.TryParse(msg.lockedUntilUtc, null, DateTimeStyles.AdjustToUniversal, out var utc))
        //     {
        //         var local = utc.ToLocalTime();
        //         SetStateIdle($"Conta bloqueada até {local:HH:mm}.");
        //         return;
        //     }
        // }
        //
        // SetStateIdle(MapAuthMessage(msg.message, msg.code));

        // Implementação compatível com AuthResponse simples (success/message):
        if (msg.success)
        {
            SetStateSuccess("Login realizado com sucesso.");
        }
        else
        {
            SetStateIdle(MapAuthMessage(msg.message, /*code*/ -1));
        }
    }

    void OnAuthFailed(string serverMessage)
    {
        SetStateIdle(MapAuthMessage(serverMessage, /*code*/ -1));
    }

    // ============ UI States ============
    void SetStateIdle(string message = "Pronto.")
    {
        SetCanvas(formGroup, true);
        SetCanvas(connectingGroup, false);
        if (buttonLogin) buttonLogin.interactable = true;
        if (userField) userField.interactable = true;
        if (passField) passField.interactable = true;
        ShowStatus(message);
    }

    void SetStateConnecting(string message)
    {
        SetCanvas(formGroup, false);
        SetCanvas(connectingGroup, true);
        ShowStatus(message);
        if (buttonLogin) buttonLogin.interactable = false;
        if (userField) userField.interactable = false;
        if (passField) passField.interactable = false;
    }

    void SetStateSuccess(string message)
    {
        // opcional: você pode deixar o connectingGroup, mas com mensagem "Autenticado"
        SetCanvas(formGroup, false);
        SetCanvas(connectingGroup, true);
        ShowStatus(message);
    }

    void SetCanvas(CanvasGroup g, bool on)
    {
        if (!g) return;
        g.alpha = on ? 1f : 0f;
        g.interactable = on;
        g.blocksRaycasts = on;
    }

    // ============ Helpers ============
    void ShowStatus(string msg)
    {
        if (statusText != null)
            statusText.text = msg;
        Debug.Log($"[LoginUIPro] {msg}");
    }

    string MapAuthMessage(string serverMessage, int code)
    {
        if (string.IsNullOrWhiteSpace(serverMessage)) return "Falha ao autenticar.";
        // Compatível com respostas simples e com enum AuthCode (se existir no seu projeto)
        // Ex.: if (code == (int)AuthCode.OK || serverMessage == "OK") ...
        if (serverMessage == "OK") return "Login realizado com sucesso.";
        if (serverMessage.Contains("Credenciais inválidas")) return "Credenciais inválidas.";

        // Sugestões se usar códigos:
        // if (code == (int)AuthCode.RateLimited) return "Muitas tentativas. Aguarde e tente novamente.";
        // if (code == (int)AuthCode.Locked)      return "Conta bloqueada temporariamente.";

        return serverMessage;
    }
}
