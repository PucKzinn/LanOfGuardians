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
                _conn.CreateTable<Account>();
                _conn.CreateTable<Character>();
            }
            return _conn;
        }
    }

    public static void Init()
    {
        // força inicialização e criação das tabelas
        var _ = Conn;
        Debug.Log($"[DB] Path: {dbPath}");
    }
}

// MODELOS (tabelas)
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
    // posição salva
    public float X { get; set; }
    public float Y { get; set; }
    public float Z { get; set; }
    public string LastLogin { get; set; }
}
