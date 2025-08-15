using System;
using System.Collections.Generic;
using Mirror;
using UnityEngine;

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
        public int code;              // AuthCode (int)
        public string lockedUntilUtc; // quando bloqueado
    }

    public static event Action<AuthResponse> AuthResponseReceived;
    public static event Action<string> ClientAuthFailed;

    // ====== RATE LIMITING SIMPLES POR IP ======
    static readonly Dictionary<string, List<DateTime>> _ipHits = new Dictionary<string, List<DateTime>>();
    const int WINDOW_SECONDS = 60;
    const int MAX_ATTEMPTS_PER_WINDOW = 20;

    bool ExceededRateLimit(string address)
    {
        if (string.IsNullOrEmpty(address)) address = "unknown";
        List<DateTime> hits;
        if (!_ipHits.TryGetValue(address, out hits))
        {
            hits = new List<DateTime>();
            _ipHits[address] = hits;
        }
        DateTime now = DateTime.UtcNow;
        DateTime cutoff = now.AddSeconds(-WINDOW_SECONDS);
        // limpa hits antigos
        hits.RemoveAll(t => t < cutoff);
        // registra
        hits.Add(now);
        return hits.Count > MAX_ATTEMPTS_PER_WINDOW;
    }

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
        string ip = conn.address;
        Debug.Log($"[Auth] Login recebido (conn={conn.connectionId} ip={ip}) user={msg.username}");

        if (ExceededRateLimit(ip))
        {
            var rl = new AuthResponse { success = false, message = "Muitas tentativas. Aguarde um pouco.", code = (int)AuthCode.RateLimited };
            conn.Send(rl);
            conn.Disconnect();
            return;
        }

        // Valida credenciais
        var res = AccountService.ValidateLoginDetailed(msg.username, msg.password);

        // Caso não exista e o cliente pedir para criar, tenta criar e validar novamente
        if (res.Code == AuthCode.Invalid && msg.createIfMissing)
        {
            if (AccountService.CreateAccount(msg.username, msg.password))
                res = AccountService.ValidateLoginDetailed(msg.username, msg.password);
        }

        if (res.Code != AuthCode.OK)
        {
            var fail = new AuthResponse {
                success = false,
                message = res.Message,
                code = (int)res.Code,
                lockedUntilUtc = res.LockedUntilUtc
            };
            Debug.LogWarning($"[Auth] Falha de login para user={msg.username} -> {res.Code}");
            conn.Send(fail);
            conn.Disconnect();
            return;
        }

        int accountId = res.AccountId;
        int charId = CharacterService.EnsureCharacter(accountId);
        conn.authenticationData = new AuthData { accountId = accountId, charId = charId };

        // envia OK para o cliente e autentica no servidor
        var ok = new AuthResponse { success = true, message = "OK", code = (int)AuthCode.OK };
        conn.Send(ok);
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
        AuthResponseReceived?.Invoke(msg);

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