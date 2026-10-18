namespace Tillegal.Application.Common.Interfaces;

public interface IPasswordHashingService
{
    string HashPassword(string password);

    bool VerifyPassword(string hashedPassword, string providedPassword);
}
