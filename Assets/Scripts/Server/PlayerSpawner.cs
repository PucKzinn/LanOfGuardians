// Assets/Scripts/Server/PlayerSpawner.cs
using Mirror;
using UnityEngine;

public class PlayerSpawner : NetworkManager
{
    public override void OnServerAddPlayer(NetworkConnectionToClient conn)
    {
        var auth = conn.authenticationData as SimpleAuthenticator.AuthData;

        Vector3 pos = GetStartPosition()?.position ?? Vector3.zero;
        if (auth != null)
        {
            var saved = CharacterService.LoadPosition(auth.charId);
            if (saved != Vector3.zero) pos = saved;
        }

        GameObject player = Instantiate(playerPrefab, pos, Quaternion.identity);
        var net = player.GetComponent<PlayerNetwork>();
        if (net != null) net.Init(auth?.charId ?? -1);

        NetworkServer.AddPlayerForConnection(conn, player);
    }

    public override void OnServerDisconnect(NetworkConnectionToClient conn)
    {
        // Em algumas versões, conn.identity pode vir null dependendo da ordem de teardown.
        if (conn != null && conn.identity != null)
        {
            var net = conn.identity.GetComponent<PlayerNetwork>();
            if (net != null) net.ServerSaveNow();
        }

        base.OnServerDisconnect(conn);
    }
}
