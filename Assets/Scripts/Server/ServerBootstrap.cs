using UnityEngine;

public class ServerBootstrap : MonoBehaviour
{
    void Awake() => Database.Init();
}
