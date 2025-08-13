// Assets/Scripts/Shared/PlayerNetwork.cs
using Mirror;
using UnityEngine;

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
}
