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
        var pn = player.GetComponent<PlayerNetwork>();
        if (pn != null) pn.Init(auth?.charId ?? -1);

        NetworkServer.AddPlayerForConnection(conn, player);

    #if UNITY_SERVER || UNITY_EDITOR
        pn?.ServerPushInventoryNow();   // ⇦ só compila no servidor/editor
    #endif
}
}