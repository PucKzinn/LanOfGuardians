using System;
using System.IO;
using UnityEngine;
using SQLite4Unity3d;

public static class Database
{
    static string dbPath;
    static SQLiteConnection _conn;
    static bool _initialized = false;

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
        if (_initialized) return;
        var _ = Conn;
        try
        {
            ApplyPragmas();
            RunMigrations();
            _initialized = true;
        }
        catch (Exception ex)
        {
            Debug.LogError("[DB] Init error: " + ex);
        }
        Debug.Log($"[DB] Path: {dbPath}");
    }

    static void ApplyPragmas()
    {
        // IMPORTANT: PRAGMAs que retornam linhas devem usar ExecuteScalar ou Query para evitar "Row" warning
        TryExecScalar("PRAGMA journal_mode=WAL;");      // retorna "wal"
        TryExec("PRAGMA busy_timeout=5000;");           // ok como Execute
        TryExecScalar("PRAGMA foreign_keys=ON;");       // retorna 1
        TryExec("PRAGMA synchronous=NORMAL;");          // ok como Execute
    }

    static void RunMigrations()
    {
        int ver = 0;
        try { ver = Conn.ExecuteScalar<int>("PRAGMA user_version;"); } catch { ver = 0; }

        if (ver < 1)
        {
            TryExec("CREATE UNIQUE INDEX IF NOT EXISTS idx_account_username ON Account(Username);");
            TryExec("CREATE INDEX IF NOT EXISTS idx_character_account ON Character(AccountId);");
            TrySetVersion(1);
        }

        if (ver < 2)
        {
            TryExec("ALTER TABLE Account ADD COLUMN FailedAttempts INTEGER DEFAULT 0;");
            TryExec("ALTER TABLE Account ADD COLUMN LockoutUntilUtc TEXT;");
            TrySetVersion(2);
        }

        if (ver < 3)
        {
            TryExec(@"
                CREATE TRIGGER IF NOT EXISTS trg_character_account_delete
                AFTER DELETE ON Account
                BEGIN
                    DELETE FROM Character WHERE AccountId = OLD.Id;
                END;");
            TrySetVersion(3);
        }
    }

    static void TrySetVersion(int v)
    {
        try { Conn.Execute($"PRAGMA user_version={v};"); } catch { }
    }

    static void TryExec(string sql)
    {
        try { Conn.Execute(sql); }
        catch (Exception ex) { Debug.LogWarning($"[DB] PRAGMA/SQL ignorado: {sql} -> {ex.Message}"); }
    }

    static void TryExecScalar(string sql)
    {
        try
        {
            var res = Conn.ExecuteScalar<string>(sql);
            Debug.Log($"[DB] {sql} => {res}");
        }
        catch (Exception ex) { Debug.LogWarning($"[DB] PRAGMA/SQL ignorado: {sql} -> {ex.Message}"); }
    }
}

// MODELOS
public class Account
{
    [PrimaryKey, AutoIncrement] public int Id { get; set; }
    [Unique] public string Username { get; set; }
    public string PassHash { get; set; }
    public string Salt { get; set; }
    public string CreatedAt { get; set; }

    public int FailedAttempts { get; set; }
    public string LockoutUntilUtc { get; set; }
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