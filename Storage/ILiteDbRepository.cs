using WebHookApp.Models;

namespace WebHookApp.Storage
{
    public interface ILiteDbRepository
    {
        int CreateLogin(Login login);
        Login GetLogin(int accountId);
        int DeleteLogin(int accountId);
        Dictionary<int, Login> GetAllLogins();
        void Dispose();
    }
}