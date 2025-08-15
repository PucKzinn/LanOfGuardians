using System;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;

public enum AuthCode
{
    OK = 0,
    Invalid = 1,
    Locked = 2,
    RateLimited = 3,
    Error = 4
}

public class AuthResult
{
    public AuthCode Code;
    public string Message;
    public int AccountId;
    public string LockedUntilUtc;
}

public static class AccountService
{
    const int MIN_PASSWORD = 8;
    const int PBKDF2_ITER = 150_000;
    const int PBKDF2_BYTES = 32;

    const int MAX_FAILED = 5;
    static readonly TimeSpan LOCK_DURATION = TimeSpan.FromMinutes(15);

    static string HashPBKDF2(string password, string salt, int iterations = PBKDF2_ITER, int bytes = PBKDF2_BYTES)
    {
        using (var pbkdf2 = new Rfc2898DeriveBytes(password, Encoding.UTF8.GetBytes(salt), iterations, HashAlgorithmName.SHA256))
        {
            var hash = pbkdf2.GetBytes(bytes);
            var sb = new StringBuilder(hash.Length * 2);
            for (int i = 0; i < hash.Length; i++)
                sb.Append(hash[i].ToString("x2"));
            return sb.ToString();
        }
    }

    static bool ConstantTimeEquals(string a, string b)
    {
        if (a == null || b == null) return false;
        if (a.Length != b.Length) return false;
        int diff = 0;
        for (int i = 0; i < a.Length; i++)
            diff |= a[i] ^ b[i];
        return diff == 0;
    }

    static string NormalizeUser(string username)
    {
        if (string.IsNullOrWhiteSpace(username)) return string.Empty;
        username = username.Trim().ToLowerInvariant();
        // permite a-z 0-9 _ . - (ajuste conforme sua necessidade)
        var cleaned = Regex.Replace(username, @"[^a-z0-9_\.\-]", "");
        return cleaned;
    }

    static bool IsValidUsername(string u) => !string.IsNullOrEmpty(u) && u.Length >= 3 && u.Length <= 20;
    static bool IsValidPassword(string p) => !string.IsNullOrEmpty(p) && p.Length >= MIN_PASSWORD;

    public static bool CreateAccount(string username, string password)
    {
        string u = NormalizeUser(username);
        if (!IsValidUsername(u) || !IsValidPassword(password)) return false;

        try
        {
            string salt = Guid.NewGuid().ToString("N");
            string passhash = HashPBKDF2(password, salt);
            var acc = new Account {
                Username = u,
                PassHash = passhash,
                Salt = salt,
                CreatedAt = DateTime.UtcNow.ToString("o"),
                FailedAttempts = 0,
                LockoutUntilUtc = null
            };
            Database.Conn.Insert(acc);
            return true;
        }
        catch
        {
            // username duplicado ou outro erro
            return false;
        }
    }

    public static int ValidateLogin(string username, string password)
    {
        var res = ValidateLoginDetailed(username, password);
        return res.Code == AuthCode.OK ? res.AccountId : -1;
    }

    public static AuthResult ValidateLoginDetailed(string username, string password)
    {
        var result = new AuthResult { Code = AuthCode.Invalid, Message = "Credenciais inválidas." };

        string u = NormalizeUser(username);
        if (!IsValidUsername(u)) { result.Message = "Usuário inválido."; return result; }
        if (!IsValidPassword(password)) { result.Message = "Senha inválida (mín. 8)."; return result; }

        var acc = Database.Conn.Table<Account>().FirstOrDefault(a => a.Username == u);
        if (acc == null)
        {
            // não existe -> permite criação se o servidor quiser
            result.Code = AuthCode.Invalid;
            result.Message = "Credenciais inválidas.";
            return result;
        }

        // Checa bloqueio
        DateTime lockUntil;
        if (!string.IsNullOrEmpty(acc.LockoutUntilUtc) && DateTime.TryParse(acc.LockoutUntilUtc, null, System.Globalization.DateTimeStyles.AdjustToUniversal, out lockUntil))
        {
            if (lockUntil > DateTime.UtcNow)
            {
                result.Code = AuthCode.Locked;
                result.Message = "Conta bloqueada temporariamente.";
                result.LockedUntilUtc = lockUntil.ToString("o");
                return result;
            }
            else
            {
                // Expirou bloqueio
                acc.LockoutUntilUtc = null;
                acc.FailedAttempts = 0;
                Database.Conn.Update(acc);
            }
        }

        string check = HashPBKDF2(password, acc.Salt);
        bool ok = ConstantTimeEquals(check, acc.PassHash);

        if (!ok)
        {
            acc.FailedAttempts = Math.Max(0, acc.FailedAttempts) + 1;
            if (acc.FailedAttempts >= MAX_FAILED)
            {
                acc.LockoutUntilUtc = DateTime.UtcNow.Add(LOCK_DURATION).ToString("o");
                acc.FailedAttempts = 0; // zera após aplicar bloqueio
                Database.Conn.Update(acc);

                result.Code = AuthCode.Locked;
                result.Message = "Conta bloqueada temporariamente.";
                result.LockedUntilUtc = acc.LockoutUntilUtc;
                return result;
            }
            else
            {
                Database.Conn.Update(acc);
                result.Code = AuthCode.Invalid;
                result.Message = "Credenciais inválidas.";
                return result;
            }
        }

        // Sucesso
        acc.FailedAttempts = 0;
        acc.LockoutUntilUtc = null;
        Database.Conn.Update(acc);

        result.Code = AuthCode.OK;
        result.Message = "OK";
        result.AccountId = acc.Id;
        return result;
    }
}