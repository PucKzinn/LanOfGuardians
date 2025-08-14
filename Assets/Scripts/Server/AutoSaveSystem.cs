using Mirror;
using UnityEngine;


public class AutoSaveSystem : MonoBehaviour
{
    const float SaveInterval = 30f;

    [ServerCallback]
    void Start() => InvokeRepeating(nameof(SaveAllPlayers), SaveInterval, SaveInterval);
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

        foreach (var conn in NetworkServer.connections.Values)
        {
            var player = conn.identity != null ? conn.identity.GetComponent<PlayerNetwork>() : null;
            if (player != null)
                player.ServerSaveNow();

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
