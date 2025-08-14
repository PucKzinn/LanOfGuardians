#if UNITY_SERVER || UNITY_EDITOR
using System.Collections.Generic;
using System.Linq;                // <- para FirstOrDefault()
using SQLite4Unity3d;

// Serviço de inventário: cria tabelas, seed, e expõe Add/Remove/List.
// Executa no servidor (ou no Editor para testes locais).
public static class InventoryService
{
    // ---- modelos de tabela ----
    [Table("items")]
    private class ItemRow
    {
        [PrimaryKey] public int item_id { get; set; }
        public string name { get; set; }
        public int stackable { get; set; }   // 0/1
        public int max_stack { get; set; }   // se stackable=1, limite por pilha
    }

    [Table("inventory")]
    private class InvRow
    {
        [PrimaryKey, AutoIncrement] public int id { get; set; }
        public int char_id { get; set; }
        public int item_id { get; set; }
        public int qty { get; set; }
    }

    // ---- tipo usado para retorno legível ----
    public class ItemEntry
    {
        public int ItemId;
        public string Name;
        public int Quantity;
    }

    static SQLiteConnection Conn => Database.Conn; // ajuste se sua conexão tiver outro nome

    public static void Init()
    {
        Conn.CreateTable<ItemRow>();
        Conn.CreateTable<InvRow>();
        SeedIfEmpty();
    }

    static void SeedIfEmpty()
    {
        // só cria itens se a tabela estiver vazia
        var count = Conn.Table<ItemRow>().Count();
        if (count > 0) return;

        // Ex.: 1 = Poção (empilhável), 2 = Espada (não empilhável)
        Conn.InsertAll(new[]
        {
            new ItemRow { item_id = 1, name = "Poção", stackable = 1, max_stack = 99 },
            new ItemRow { item_id = 2, name = "Espada", stackable = 0, max_stack = 1 },
        });
    }

    public static List<ItemEntry> ListItems(int charId)
    {
        // join simples para listar nome e quantidade
        string sql = @"
            SELECT i.item_id as ItemId, i.name as Name, IFNULL(v.qty, 0) as Quantity
            FROM items i
            LEFT JOIN inventory v ON v.item_id = i.item_id AND v.char_id = ?
            WHERE IFNULL(v.qty,0) > 0
            ORDER BY i.item_id";
        return Conn.Query<ItemEntry>(sql, charId);
    }

    public static void AddItem(int charId, int itemId, int qty)
    {
        if (qty <= 0) return;

        var def = Conn.Find<ItemRow>(itemId);
        if (def == null) return;

        // existe entry?
        var existing = Conn.Query<InvRow>(
            "SELECT * FROM inventory WHERE char_id=? AND item_id=? LIMIT 1",
            charId, itemId
        ).FirstOrDefault();

        int newQty = qty + (existing?.qty ?? 0);

        // respeita stack se definido
        if (def.stackable == 1 && def.max_stack > 0 && newQty > def.max_stack)
            newQty = def.max_stack;

        if (existing == null)
        {
            Conn.Insert(new InvRow { char_id = charId, item_id = itemId, qty = newQty });
        }
        else
        {
            existing.qty = newQty;
            Conn.Update(existing);
        }
    }

    public static void RemoveItem(int charId, int itemId, int qty)
    {
        if (qty <= 0) return;

        var existing = Conn.Query<InvRow>(
            "SELECT * FROM inventory WHERE char_id=? AND item_id=? LIMIT 1",
            charId, itemId
        ).FirstOrDefault();

        if (existing == null) return;

        existing.qty -= qty;
        if (existing.qty <= 0)
        {
            Conn.Execute("DELETE FROM inventory WHERE id=?", existing.id);
        }
        else
        {
            Conn.Update(existing);
        }
    }
}
#endif
