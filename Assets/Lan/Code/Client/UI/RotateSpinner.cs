
using UnityEngine;

/// <summary>
/// Rotaciona um RectTransform continuamente para servir de "spinner/loading".
/// </summary>
public class RotateSpinner : MonoBehaviour
{
    public RectTransform target;
    public float speed = 180f; // graus por segundo

    void Reset()
    {
        if (!target) target = transform as RectTransform;
    }

    void Update()
    {
        if (!target) return;
        target.Rotate(0f, 0f, -speed * Time.deltaTime, Space.Self);
    }
}
