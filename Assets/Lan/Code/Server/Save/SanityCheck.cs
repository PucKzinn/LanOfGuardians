using UnityEngine;
using System.IO;

public class SanityCheck : MonoBehaviour
{
    void Awake()
    {
        Debug.Log("[Sanity] Unity Version: " + Application.unityVersion);
#if UNITY_EDITOR
        Debug.Log("[Sanity] Editor? Sim");
#endif
        Debug.Log("[Sanity] persistentDataPath: " + Application.persistentDataPath);
    }

    void Start()
    {
        // força inicialização do DB
        Database.Init();
        Debug.Log("[Sanity] DB inicializado");
    }
}