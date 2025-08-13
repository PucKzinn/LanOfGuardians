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
        player.GetComponent<PlayerNetwork>().Init(auth?.charId ?? -1);
        NetworkServer.AddPlayerForConnection(conn, player);
    }
}
