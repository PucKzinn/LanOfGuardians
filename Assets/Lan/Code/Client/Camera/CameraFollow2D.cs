using Mirror;
using UnityEngine;

public class CameraFollow2D : MonoBehaviour
{
    Transform target;

    void LateUpdate()
    {
        if (target == null)
        {
            if (NetworkClient.localPlayer == null) return;
            target = NetworkClient.localPlayer.transform;
        }

        Vector3 position = target.position;
        position.z = transform.position.z;
        transform.position = position;
    }
}
