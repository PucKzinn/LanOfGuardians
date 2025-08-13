using Mirror;
using UnityEngine;
using System;

public class SimpleAuthenticator : NetworkAuthenticator
{
    // Cache preenchido pela UI ANTES de conectar
    [HideInInspector] public string cachedUsername;
    [HideInInspector] public string cachedPassword;
    [HideInInspector] public bool cachedCreate;

    public struct LoginMessage : NetworkMessage
    {
        public string username;
        public string password;
        public bool createIfMissing;
    }

    public struct AuthResponse : NetworkMessage
    {
        public bool success;
        public string message;
    }

    public static event Action<string> ClientAuthFailed;

    // ===== SERVER =====
    public override void OnStartServer()
    {
        Debug.Log("[Auth] OnStartServer: registrando LoginMessage");
        NetworkServer.RegisterHandler<LoginMessage>(OnLoginServer, false);
    }
    public override void OnStopServer()
    {
        NetworkServer.UnregisterHandler<LoginMessage>();
    }

    void OnLoginServer(NetworkConnectionToClient conn, LoginMessage msg)
    {
        Debug.Log($"[Auth] Login recebido (conn={conn.connectionId}) user={msg.username}");

        int accountId = AccountService.ValidateLogin(msg.username, msg.password);
        if (accountId < 0 && msg.createIfMissing)
        {
            if (AccountService.CreateAccount(msg.username, msg.password))
                accountId = AccountService.ValidateLogin(msg.username, msg.password);
        }

        if (accountId < 0)
        {
            Debug.LogWarning($"[Auth] Falha de login para user={msg.username}");
            conn.Send(new AuthResponse { success = false, message = "Credenciais inválidas." });
            conn.Disconnect();
            return;
        }

        int charId = CharacterService.EnsureCharacter(accountId);
        conn.authenticationData = new AuthData { accountId = accountId, charId = charId };

        // envia OK para o cliente e autentica no servidor
        conn.Send(new AuthResponse { success = true, message = "OK" });
        OnServerAuthenticated.Invoke(conn);
    }

    // ===== CLIENT =====
    public override void OnStartClient()
    {
        Debug.Log("[Auth] OnStartClient: registrando AuthResponse");
        NetworkClient.RegisterHandler<AuthResponse>(OnAuthResponse, false);
    }
    public override void OnStopClient()
    {
        NetworkClient.UnregisterHandler<AuthResponse>();
    }

    // AGORA: o envio do login fica aqui
    public override void OnClientAuthenticate()
    {
        Debug.Log("[Auth] OnClientAuthenticate: enviando LoginMessage");
        var msg = new LoginMessage {
            username = cachedUsername ?? "",
            password = cachedPassword ?? "",
            createIfMissing = cachedCreate
        };
        NetworkClient.Send(msg);
    }

    void OnAuthResponse(AuthResponse msg)
    {
        if (msg.success)
        {
            Debug.Log("[Auth] Cliente autenticado (recebeu OK)");
            OnClientAuthenticated.Invoke();
        }
        else
        {
            Debug.LogWarning("[Auth] Falha de autenticação no cliente: " + msg.message);
            ClientAuthFailed?.Invoke(msg.message);
            NetworkClient.Disconnect();
        }
    }

    public class AuthData { public int accountId; public int charId; }
}
