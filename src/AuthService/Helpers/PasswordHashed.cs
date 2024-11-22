namespace AuthService.Helpers;


public interface IPasswordHashedHelper
{
    string HashPassword(string password);
    bool ValidatePassword(string password, string correctHash);
}

public class PasswordHashedHelper : IPasswordHashedHelper
{
    public string HashPassword(string password)
    {
        var salt = BCrypt.Net.BCrypt.GenerateSalt(12);
        return BCrypt.Net.BCrypt.HashPassword(password, salt);
    }

    public bool ValidatePassword(string password, string correctHash)
    {
        return BCrypt.Net.BCrypt.Verify(password, correctHash);
    }
}
