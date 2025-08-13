using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public static class InventoryService
{
    public class ItemEntry
    {
        public int ItemId { get; set; }
        public string Name { get; set; }
        public int Quantity { get; set; }
    }

    public static void Seed()
    {
        // cria itens básicos se a tabela estiver vazia
        if (Database.Conn.Table<Item>().Count() != 0) return;
        Debug.unityLogger?.Log(LogType.Log, "[Inventory] seeding items");
        Database.Conn.Insert(new Item { Name = "Potion" });
        Database.Conn.Insert(new Item { Name = "Sword" });
    }

    public static void AddItem(int charId, int itemId, int qty)
    {
        if (qty <= 0) throw new ArgumentOutOfRangeException(nameof(qty));
        var inv = Database.Conn.Table<Inventory>()
            .FirstOrDefault(i => i.CharacterId == charId && i.ItemId == itemId);
        if (inv == null)
        {
            inv = new Inventory { CharacterId = charId, ItemId = itemId, Quantity = qty };
            Database.Conn.Insert(inv);
        }
        else
        {
            inv.Quantity += qty;
            Database.Conn.Update(inv);
        }
    }

    public static void RemoveItem(int charId, int itemId, int qty)
    {
        if (qty <= 0) throw new ArgumentOutOfRangeException(nameof(qty));
        var inv = Database.Conn.Table<Inventory>()
            .FirstOrDefault(i => i.CharacterId == charId && i.ItemId == itemId);
        if (inv == null) return;
        inv.Quantity -= qty;
        if (inv.Quantity <= 0)
            Database.Conn.Delete(inv);
        else
            Database.Conn.Update(inv);
    }

    public static List<ItemEntry> ListItems(int charId)
    {
        var items = Database.Conn.Table<Item>().ToList();
        var result = new List<ItemEntry>();
        foreach (var item in items)
        {
            var inv = Database.Conn.Table<Inventory>()
                .FirstOrDefault(i => i.CharacterId == charId && i.ItemId == item.Id);
            result.Add(new ItemEntry
            {
                ItemId = item.Id,
                Name = item.Name,
                Quantity = inv?.Quantity ?? 0
            });
        }
        return result;
    }
}
