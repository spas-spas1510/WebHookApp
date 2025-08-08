
namespace WebHookApp.Models
{
    [Serializable]
    public class Login
    {
        public int AccountId { get; set; }
        public string Password { get; set; }
        public string Server { get; set; }
        public Guid Id { get; set; }
    }
}
