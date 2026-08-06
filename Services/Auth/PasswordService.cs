using TaskManager.Models.Auth;
using Microsoft.AspNetCore.Identity;

public class PasswordService
{
    private readonly PasswordHasher<Account> _hasher = new();

    public string HashPassword(string password)
    {
        return _hasher.HashPassword(null!, password); // 'null' if user isn't required
    }

    public bool VerifyPassword(string hashedPassword, string providedPassword)
    {
        var result = _hasher.VerifyHashedPassword(null!, hashedPassword, providedPassword);
        return result == PasswordVerificationResult.Success;
    }
}
