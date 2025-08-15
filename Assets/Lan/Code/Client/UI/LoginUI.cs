using System;
using System.Globalization;
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
    public InputField addressField;   // opcional (IP/Host)
    public Text statusText;           // opcional (UI de status)

    [Header("Referências")]
    public NetworkManager manager;            // arraste o NetworkManager (PlayerSpawner)
    public SimpleAuthenticator authenticator; // arraste o SimpleAuthenticator do mesmo objeto

    void Awake()
    {
        if (manager == null) manager = NetworkManager.singleton;
        ShowStatus("Pronto.");
    }

    void OnEnable()
    {
        // Assina os dois eventos uma única vez
        SimpleAuthenticator.AuthResponseReceived += OnAuthResponse;
        SimpleAuthenticator.ClientAuthFailed     += OnAuthFailed;
    }

    void OnDisable()
    {
        // Desassina para evitar leaks
        SimpleAuthenticator.AuthResponseReceived -= OnAuthResponse;
        SimpleAuthenticator.ClientAuthFailed     -= OnAuthFailed;
    }

    public void Connect()
    {
        if (manager == null) manager = NetworkManager.singleton;
        if (authenticator == null)
        {
            ShowStatus("Authenticator não atribuído no NetworkManager.");
            return;
        }

        string username = GetUser();
        string password = GetPass();
        bool createIfMissing = createIfMissingToggle != null && createIfMissingToggle.isOn;

        if (string.IsNullOrWhiteSpace(username)) { ShowStatus("Usuário vazio."); return; }
        if (string.IsNullOrWhiteSpace(password)) { ShowStatus("Senha vazia.");   return; }

        if (addressField != null && !string.IsNullOrWhiteSpace(addressField.text))
            manager.networkAddress = addressField.text.Trim();

        // Entrega as credenciais ao Authenticator; ele envia no OnClientAuthenticate()
        authenticator.cachedUsername = username;
        authenticator.cachedPassword = password;
        authenticator.cachedCreate   = createIfMissing;

        ShowStatus($"Conectando em {manager.networkAddress}...");
        manager.StartClient();
    }

    // ---- Handlers de autenticação (eventos do SimpleAuthenticator) ----
    void OnAuthResponse(SimpleAuthenticator.AuthResponse msg)
    {
        // Se veio bloqueio, mostra hora local
        if (msg.code == (int)AuthCode.Locked && !string.IsNullOrEmpty(msg.lockedUntilUtc))
        {
            DateTime utc;
            if (DateTime.TryParse(msg.lockedUntilUtc, null, DateTimeStyles.AdjustToUniversal, out utc))
            {
                var local = utc.ToLocalTime();
                ShowStatus($"Conta bloqueada até {local:HH:mm}.");
                return;
            }
        }

        ShowStatus(MapAuthMessage(msg.message, msg.code));
    }

    void OnAuthFailed(string serverMessage)
    {
        ShowStatus(MapAuthMessage(serverMessage, (int)AuthCode.Error));
    }

    // ---- Helpers de UI ----
    string GetUser() => userFieldTMP != null ? userFieldTMP.text.Trim() : "";
    string GetPass() => passFieldTMP != null ? passFieldTMP.text        : "";

    void ShowStatus(string msg)
    {
        if (statusText != null) statusText.text = msg;
        Debug.Log($"[LoginUI] {msg}");
    }

    string MapAuthMessage(string serverMessage, int code)
    {
        if (string.IsNullOrWhiteSpace(serverMessage)) return "Falha ao autenticar.";
        if (code == (int)AuthCode.OK || serverMessage == "OK") return "Login realizado com sucesso.";
        if (code == (int)AuthCode.RateLimited) return "Muitas tentativas. Aguarde um pouco e tente novamente.";
        if (code == (int)AuthCode.Locked) return "Conta bloqueada temporariamente.";
        if (serverMessage.Contains("Credenciais inválidas")) return "Credenciais inválidas.";
        return serverMessage;
    }
}