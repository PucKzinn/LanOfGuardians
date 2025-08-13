using Mirror;
using UnityEngine;

public class PlayerNetwork : NetworkBehaviour
{
    int charId = -1;
    public void Init(int id) => charId = id;

    void OnDestroy()
    {
        if (isServer && charId > 0)
            CharacterService.SavePosition(charId, transform.position);
    }
}
