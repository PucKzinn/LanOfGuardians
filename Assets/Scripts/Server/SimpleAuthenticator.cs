using Mirror;
using UnityEngine;

public class SimpleAuthenticator : NetworkAuthenticator
{
    public struct LoginMessage : NetworkMessage
    {
        public string username;
        public string password;
        public bool createIfMissing;
    }

    public override void OnStartServer() => NetworkServer.RegisterHandler<LoginMessage>(OnLoginServer);
    public override void OnStopServer() => NetworkServer.UnregisterHandler<LoginMessage>();
    public override void OnStartClient() { }

    public override void OnClientAuthenticate() { }

    void OnLoginServer(NetworkConnectionToClient conn, LoginMessage msg)
    {
        int accountId = AccountService.ValidateLogin(msg.username, msg.password);
        if (accountId < 0 && msg.createIfMissing)
        {
            if (AccountService.CreateAccount(msg.username, msg.password))
                accountId = AccountService.ValidateLogin(msg.username, msg.password);
        }

        if (accountId < 0)
        {
            conn.Disconnect();
            return;
        }

        int charId = CharacterService.EnsureCharacter(accountId);
        conn.authenticationData = new AuthData { accountId = accountId, charId = charId };
        ServerAccept(conn);
    }

    void ServerAccept(NetworkConnectionToClient conn) => OnServerAuthenticated.Invoke(conn);

    public class AuthData { public int accountId; public int charId; }
}
