using System.IO;
using UnityEngine;
using SQLite4Unity3d;

public static class Database
{
    static string dbPath;
    static SQLiteConnection _conn;

    public static SQLiteConnection Conn
    {
        get
        {
            if (_conn == null)
            {
                dbPath = Path.Combine(Application.persistentDataPath, "game.db");
                _conn = new SQLiteConnection(dbPath, SQLiteOpenFlags.ReadWrite | SQLiteOpenFlags.Create);

                // Tabelas “core” (contas/personagens) ficam aqui:
                _conn.CreateTable<Account>();
                _conn.CreateTable<Character>();
                // ⚠️ NÃO crie “items” e “inventory” aqui.
                // Elas são responsabilidade do InventoryService.
            }
            return _conn;
        }
    }

    // <<< Recrie este método >>>
    public static void Init()
    {
        // Força abrir/conectar e criar tabelas Account/Character:
        var _ = Conn;

        Debug.Log($"[DB] Path: {dbPath}");
    }
}

// MODELOS CORE
public class Account
{
    [PrimaryKey, AutoIncrement] public int Id { get; set; }
    [Unique] public string Username { get; set; }
    public string PassHash { get; set; }
    public string Salt { get; set; }
    public string CreatedAt { get; set; }
}

public class Character
{
    [PrimaryKey, AutoIncrement] public int Id { get; set; }
    public int AccountId { get; set; }
    public string Name { get; set; }
    public float X { get; set; }
    public float Y { get; set; }
    public float Z { get; set; }
    public string LastLogin { get; set; }
}
