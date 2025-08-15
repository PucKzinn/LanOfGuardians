using Mirror;
using UnityEngine;

/// <summary>
/// Periodically saves all connected players' positions.
/// </summary>
public class AutoSaveSystem : MonoBehaviour
{
    [SerializeField] float intervalSeconds = 30f;

    void Start()
    {
        if (intervalSeconds <= 0f) intervalSeconds = 30f;
        InvokeRepeating(nameof(SaveAllPlayers), intervalSeconds, intervalSeconds);
    }

    void OnDestroy()
    {
        CancelInvoke();
    }

    [Server]
    void SaveAllPlayers()
    {
        if (!NetworkServer.active) return;

        foreach (var conn in NetworkServer.connections.Values)
        {
            if (conn?.identity == null) continue;
            var player = conn.identity.GetComponent<PlayerNetwork>();
            player?.ServerSaveNow();
        }
    }
}
