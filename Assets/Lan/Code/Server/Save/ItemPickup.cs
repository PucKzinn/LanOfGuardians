using UnityEngine;
using Mirror;

[RequireComponent(typeof(Collider2D))]
public class ItemPickup : NetworkBehaviour
{
    [Header("Definição do item")]
    public int itemId = 1;   // 1 = Poção (do seu seed)
    public int amount = 1;

    [Header("Pickup")]
    public bool destroyOnPickup = true;

    void Reset()
    {
        var col = GetComponent<Collider2D>();
        if (col) col.isTrigger = true;
    }

    [ServerCallback]
    void OnTriggerEnter2D(Collider2D other)
    {
        if (!isServer) return;

        var pn = other.GetComponent<PlayerNetwork>();
        if (pn == null) return;

        pn.ServerGiveItem(itemId, amount);

        if (destroyOnPickup)
            NetworkServer.Destroy(gameObject);
    }
}
