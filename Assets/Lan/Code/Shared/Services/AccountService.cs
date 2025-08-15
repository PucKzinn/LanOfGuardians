using System;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

public static class AccountService
{
    static string HashPBKDF2(string password, string salt, int iterations = 100_000, int bytes = 32)
    {
        using var pbkdf2 = new System.Security.Cryptography.Rfc2898DeriveBytes(
            password, System.Text.Encoding.UTF8.GetBytes(salt), iterations, System.Security.Cryptography.HashAlgorithmName.SHA256);
        return System.BitConverter.ToString(pbkdf2.GetBytes(bytes)).Replace("-", "").ToLowerInvariant();
    }

    public static bool CreateAccount(string username, string password)
    {
        try
        {
            string salt = Guid.NewGuid().ToString("N");
            string passhash = HashPBKDF2(password, salt);
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
        string check = HashPBKDF2(password, acc.Salt);
        return check == acc.PassHash ? acc.Id : -1;
    }
}
