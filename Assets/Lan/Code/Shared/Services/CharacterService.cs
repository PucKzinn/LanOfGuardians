using System;
using System.Linq;
using UnityEngine;

public static class CharacterService
{
    public static int EnsureCharacter(int accountId, string name = null)
    {
        var existing = Database.Conn.Table<Character>().FirstOrDefault(c => c.AccountId == accountId);
        if (existing != null) return existing.Id;

        var ch = new Character {
            AccountId = accountId,
            Name = name ?? $"Hero_{accountId}",
            X = 0, Y = 0, Z = 0,
            LastLogin = DateTime.UtcNow.ToString("o")
        };
        Database.Conn.Insert(ch);
        return ch.Id;
    }

    public static Vector3 LoadPosition(int charId)
    {
        var ch = Database.Conn.Find<Character>(charId);
        if (ch == null) return Vector3.zero;
        return new Vector3(ch.X, ch.Y, ch.Z);
    }

    public static void SavePosition(int charId, Vector3 pos)
    {
        var ch = Database.Conn.Find<Character>(charId);
        if (ch == null) return;
        ch.X = pos.x; ch.Y = pos.y; ch.Z = pos.z;
        ch.LastLogin = DateTime.UtcNow.ToString("o");
        Database.Conn.Update(ch);
    }
}
