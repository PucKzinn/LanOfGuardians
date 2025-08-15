using UnityEngine;

public class ServerBootstrap : MonoBehaviour
{
    void Awake()
    {
        Database.Init();
        gameObject.AddComponent<AutoSaveSystem>();
    }
}
