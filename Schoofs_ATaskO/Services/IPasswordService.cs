namespace Schoofs_ATaskO.Services
{
    public interface IPasswordService
    {
        string GenerateSalt();
        string HashPassword(string password, string saltBase64);
        bool VerifyPassword(string password, string saltBase64, string expectedHashBase64);
    }
}
