// Assets/Scripts/Shared/PlayerNetwork.cs
using Mirror;
using UnityEngine;
using System.Collections.Generic;


public class PlayerNetwork : NetworkBehaviour
{
    int charId = -1;
    public void Init(int id) => charId = id;

    [Server]
    public void ServerSaveNow()
    {
        if (charId > 0)
            CharacterService.SavePosition(charId, transform.position);
    }

    public override void OnStopServer()
    {
        ServerSaveNow();
    }

    // opcional (redundante, mas ajuda em algumas ordens de teardown)
    void OnDestroy()
    {
        if (isServer) ServerSaveNow();
    }

    [Command]
    public void CmdSendChat(ChatMessage message)
    {
        if (string.IsNullOrWhiteSpace(message.text)) return;
        RpcReceiveChat(message);
    }

    [ClientRpc]
    void RpcReceiveChat(ChatMessage message)
    {
        ChatUI.Instance?.Receive(message);
    }

    // ===== INVENTÁRIO =====
    [Command]
    public void CmdAddItem(int itemId, int qty)
    {
        #if UNITY_SERVER || UNITY_EDITOR
        if (charId <= 0) return;
        InventoryService.AddItem(charId, itemId, qty);
        TargetInventoryUpdated(connectionToClient, BuildDTO(charId));
        #endif
    }

    [Command]
    public void CmdRemoveItem(int itemId, int qty)
    {
        #if UNITY_SERVER || UNITY_EDITOR
        if (charId <= 0) return;
        InventoryService.RemoveItem(charId, itemId, qty);
        TargetInventoryUpdated(connectionToClient, BuildDTO(charId));
        #endif
    }

    [TargetRpc]
    void TargetInventoryUpdated(NetworkConnection target, ItemDTO[] items)
    {
        InventoryUI_TMP.Refresh(items); // UI do cliente
    }

    #if UNITY_SERVER || UNITY_EDITOR
    ItemDTO[] BuildDTO(int cid)
    {
        var list = InventoryService.ListItems(cid);
        var dto = new List<ItemDTO>(list.Count);
        foreach (var it in list)
            dto.Add(new ItemDTO { id = it.ItemId, name = it.Name, qty = it.Quantity });
        return dto.ToArray();
    }

    [Server]
    public void ServerPushInventoryNow()
    {
        if (charId > 0)
            TargetInventoryUpdated(connectionToClient, BuildDTO(charId));
    }
    #endif
    [Server]
    public void ServerGiveItem(int itemId, int qty)
    {
#if UNITY_SERVER || UNITY_EDITOR
        if (qty <= 0) return;
        // charId já está setado por Init(id) no spawn
        if (charId <= 0) return;

        InventoryService.AddItem(charId, itemId, qty);
        TargetInventoryUpdated(connectionToClient, BuildDTO(charId));
#endif
    }
}