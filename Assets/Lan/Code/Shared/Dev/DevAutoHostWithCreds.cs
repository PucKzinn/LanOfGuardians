#if UNITY_EDITOR
using UnityEngine;
using Mirror;

public class DevAutoHostWithCreds : MonoBehaviour
{
    [Header("Credenciais de teste")]
    public string username = "dev";
    public string password = "dev";
    public bool createIfMissing = true;

    void Start()
    {
        var nm = NetworkManager.singleton as PlayerSpawner; // seu NetworkManager custom
        if (nm == null) nm = FindObjectOfType<PlayerSpawner>();
        if (nm == null) { Debug.LogWarning("PlayerSpawner não encontrado."); return; }

        var auth = nm.authenticator as SimpleAuthenticator;
        if (auth == null) { Debug.LogWarning("SimpleAuthenticator não encontrado."); return; }

        // Preenche o cache do autenticador para o cliente local do Host
        auth.cachedUsername = username;
        auth.cachedPassword = password;
        auth.cachedCreate   = createIfMissing;

        // Sobe Host (Server + Client local)
        nm.StartHost();
        Debug.Log("[Dev] StartHost com credenciais de teste.");
    }
}
#endif
