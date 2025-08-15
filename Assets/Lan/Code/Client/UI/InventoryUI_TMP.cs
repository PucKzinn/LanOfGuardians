using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class InventoryUI_TMP : MonoBehaviour
{
    [Header("Refs (arraste no Inspector)")]
    public Transform listRoot;   // Content com VerticalLayoutGroup
    public GameObject rowPrefab; // Prefab com InventoryRowUI

    static InventoryUI_TMP _instance;

    // Pool simples para evitar GC por recriação de linhas
    readonly List<InventoryRowUI> _activeRows = new List<InventoryRowUI>();
    readonly Stack<InventoryRowUI> _pool = new Stack<InventoryRowUI>();

    void Awake() => _instance = this;

    /// <summary>
    /// Atualiza a UI do inventário com os dados do servidor.
    /// </summary>
    public static void Refresh(ItemDTO[] items)
    {
        if (_instance == null || _instance.listRoot == null || _instance.rowPrefab == null)
            return;

        _instance.InternalRefresh(items);
    }

    void InternalRefresh(ItemDTO[] items)
    {
        // Devolve todas as linhas ativas para o pool
        for (int i = 0; i < _activeRows.Count; i++)
        {
            var row = _activeRows[i];
            row.gameObject.SetActive(false);
            _pool.Push(row);
        }
        _activeRows.Clear();

        if (items == null || items.Length == 0) return;

        // Cria/pega do pool e preenche
        foreach (var it in items)
        {
            var row = GetRow();
            row.Setup(it);
            _activeRows.Add(row);
        }
    }

    InventoryRowUI GetRow()
    {
        InventoryRowUI row;
        if (_pool.Count > 0)
        {
            row = _pool.Pop();
            row.gameObject.SetActive(true);
        }
        else
        {
            var go = Instantiate(rowPrefab, listRoot);
            row = go.GetComponent<InventoryRowUI>();
            if (row == null)
            {
                // Garantir que o prefab tenha o componente
                row = go.AddComponent<InventoryRowUI>();
            }
        }
        return row;
    }
}
