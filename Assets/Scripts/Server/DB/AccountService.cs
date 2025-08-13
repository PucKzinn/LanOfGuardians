using System;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

public static class AccountService
{
    static string Hash(string password, string salt)
    {
        using (var sha = SHA256.Create())
        {
            var bytes = Encoding.UTF8.GetBytes(password + salt);
            var hash = sha.ComputeHash(bytes);
            return BitConverter.ToString(hash).Replace("-", "").ToLowerInvariant();
        }
    }

    public static bool CreateAccount(string username, string password)
    {
        try
        {
            string salt = Guid.NewGuid().ToString("N");
            string passhash = Hash(password, salt);
            var acc = new Account {
                Username = username,
                PassHash = passhash,
                Salt = salt,
                CreatedAt = DateTime.UtcNow.ToString("o")
            };
            Database.Conn.Insert(acc);
            return true;
        }
        catch
        {
            return false; // username duplicado ou outro erro
        }
    }

    public static int ValidateLogin(string username, string password)
    {
        var acc = Database.Conn.Table<Account>().FirstOrDefault(a => a.Username == username);
        if (acc == null) return -1;
        string check = Hash(password, acc.Salt);
        return check == acc.PassHash ? acc.Id : -1;
    }
}
