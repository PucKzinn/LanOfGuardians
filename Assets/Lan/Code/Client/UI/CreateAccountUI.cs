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
    public Toggle hostModeToggle;     // Host (dev)
    public Toggle showPassToggle;     // Mostrar senha

    [Header("UI")]
    public TextMeshProUGUI statusTMP;
    public Button createButton;
    public Button backButton;
    public GameObject spinner;

    [Header("Navegação")]
    public LoginScreenManager screenManager; // voltar para conectar

    [Header("Rede")]
    public NetworkManager manager;
    public SimpleAuthenticator authenticator;

    void Awake()
    {
        if (manager == null) manager = NetworkManager.singleton;
        ShowStatus("Crie sua conta.");

        // Semente com último usuário (opcional)
        string last = PlayerPrefs.GetString("last_user", "");
        if (!string.IsNullOrWhiteSpace(last) && userField) userField.text = last;

        if (showPassToggle != null)
        {
            showPassToggle.onValueChanged.AddListener(OnShowPassChanged);
            OnShowPassChanged(showPassToggle.isOn);
        }

        if (spinner) spinner.SetActive(false);
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

        // Criar se faltar
        authenticator.cachedUsername = user;
        authenticator.cachedPassword = pass;
        authenticator.cachedCreate   = true;

        SetUIBusy(true);

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
        if (msg.success && userField)
        {
            PlayerPrefs.SetString("last_user", userField.text.Trim());
            PlayerPrefs.Save();
        }
        ShowStatus(MapAuthMessage(msg.message));
        SetUIBusy(false);
    }

    void OnAuthFailed(string serverMessage)
    {
        ShowStatus(MapAuthMessage(serverMessage));
        SetUIBusy(false);
    }

    void SetUIBusy(bool busy)
    {
        if (spinner) spinner.SetActive(busy);

        if (createButton)    createButton.interactable    = !busy;
        if (backButton)      backButton.interactable      = !busy;
        if (userField)       userField.interactable       = !busy;
        if (passField)       passField.interactable       = !busy;
        if (confirmField)    confirmField.interactable    = !busy;
        if (addressFieldTMP) addressFieldTMP.interactable = !busy;
        if (hostModeToggle)  hostModeToggle.interactable  = !busy;
        if (showPassToggle)  showPassToggle.interactable  = !busy;
    }

    void OnShowPassChanged(bool show)
    {
        if (passField != null)
        {
            passField.contentType = show ? TMP_InputField.ContentType.Standard
                                         : TMP_InputField.ContentType.Password;
            passField.ForceLabelUpdate();
        }
        if (confirmField != null)
        {
            confirmField.contentType = show ? TMP_InputField.ContentType.Standard
                                            : TMP_InputField.ContentType.Password;
            confirmField.ForceLabelUpdate();
        }
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
