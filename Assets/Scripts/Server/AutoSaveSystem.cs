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

        foreach (var kvp in NetworkServer.connections)
        {
            var conn = kvp.Value;
            if (conn?.identity == null) continue;
            var net = conn.identity.GetComponent<PlayerNetwork>();
            net?.ServerSaveNow();
        }
    }
}
