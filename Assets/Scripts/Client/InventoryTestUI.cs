using UnityEngine;
using System.Collections.Generic;

public class InventoryTestUI : MonoBehaviour
{
    int charId;

    void Start()
    {
        Database.Init();
        charId = CharacterService.EnsureCharacter(1);
    }

    void OnGUI()
    {
        GUILayout.BeginVertical("box");
        GUILayout.Label("Inventory Test");

        List<InventoryService.ItemEntry> items = InventoryService.ListItems(charId);
        foreach (var item in items)
        {
            GUILayout.BeginHorizontal();
            GUILayout.Label($"{item.Name}: {item.Quantity}");
            if (GUILayout.Button("+")) InventoryService.AddItem(charId, item.ItemId, 1);
            if (GUILayout.Button("-")) InventoryService.RemoveItem(charId, item.ItemId, 1);
            GUILayout.EndHorizontal();
        }

        GUILayout.EndVertical();
    }
}
