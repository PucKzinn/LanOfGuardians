using Mirror;
using UnityEngine;

public class AutoSaveSystem : MonoBehaviour
{
    const float SaveInterval = 30f;

    [ServerCallback]
    void Start() => InvokeRepeating(nameof(SaveAllPlayers), SaveInterval, SaveInterval);

    [Server]
    void SaveAllPlayers()
    {
        foreach (var conn in NetworkServer.connections.Values)
        {
            var player = conn.identity != null ? conn.identity.GetComponent<PlayerNetwork>() : null;
            if (player != null)
                player.ServerSaveNow();
        }
    }
}
