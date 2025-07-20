using WebHookApp.Models;
using LiteDB;

namespace WebHookApp.Storage
{
    public class LiteDbRepository : IDisposable, ILiteDbRepository
    {
        private readonly ILiteDatabase _db;
        private readonly string LoginsCollectionName = "logins";
        private readonly ILiteCollection<Login> loginsCollection;

        public LiteDbRepository(ILiteDatabase liteDatabase)
        {
            _db = liteDatabase;
            loginsCollection = _db.GetCollection<Login>(LoginsCollectionName);
        }

        public int CreateLogin(Login login)
        {            
            return loginsCollection.Insert(login);
        }

        public Login GetLogin(int accountId)
        {
            return loginsCollection.FindOne(x => x.AccountId == accountId);
        }

        public int DeleteLogin(int accountId)
        {
            return loginsCollection.DeleteMany(x => x.AccountId == accountId);
        }

        public Dictionary<int, Login> GetAllLogins()
        {
            return loginsCollection.FindAll().ToDictionary(x => x.AccountId, y => y);
        }

        public void Dispose()
        {
            _db?.Dispose();
        }
    }
}
