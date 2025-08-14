using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Mirror;

public class InventoryRowUI : MonoBehaviour
{
    [Header("Refs (arraste no Inspector)")]
    public TMP_Text nameLabel;
    public TMP_Text qtyLabel;
    public Button addButton;       // opcional
    public Button removeButton;    // opcional

    int _itemId;

    // Chame ao criar/atualizar a linha
    public void Setup(ItemDTO data)
    {
        _itemId = data.id;

        if (nameLabel) nameLabel.text = data.name ?? "";
        if (qtyLabel)  qtyLabel.text  = "x" + data.qty;

        // Limpa listeners anteriores para evitar múltiplos binds
        if (addButton)
        {
            addButton.onClick.RemoveAllListeners();
            addButton.onClick.AddListener(OnAddClicked);
        }

        if (removeButton)
        {
            removeButton.onClick.RemoveAllListeners();
            removeButton.onClick.AddListener(OnRemoveClicked);
        }

        // Exemplo: desabilitar Remove quando qty = 0
        if (removeButton) removeButton.interactable = data.qty > 0;
    }

    void OnAddClicked()
    {
        var lp = NetworkClient.localPlayer;
        var pn = lp ? lp.GetComponent<PlayerNetwork>() : null;
        pn?.CmdAddItem(_itemId, 1);
    }

    void OnRemoveClicked()
    {
        var lp = NetworkClient.localPlayer;
        var pn = lp ? lp.GetComponent<PlayerNetwork>() : null;
        pn?.CmdRemoveItem(_itemId, 1);
    }
}
