using UnityEngine;
using TMPro;
using Mirror;

public class NPCDialogUI : MonoBehaviour
{
    public static NPCDialogUI Instance;
    public TMP_Text text;

    void Awake() => Instance = this;

    public void Show(string message)
    {
        if (text) text.text = message;
        gameObject.SetActive(true);
    }

    public void Hide() => gameObject.SetActive(false);

    // Botão "Comprar Poção"
    public void OnBuyPotion()
    {
        var lp = NetworkClient.localPlayer;
        var pn = lp ? lp.GetComponent<PlayerNetwork>() : null;
        pn?.CmdAddItem(1, 1);  // usa seu Cmd existente para adicionar poção
        Hide();
    }
}
