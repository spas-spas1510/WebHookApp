using System.Runtime.InteropServices;

namespace WebHookApp.Models
{
    [Serializable]
    public class Login
    {
        public int AccountId { get; set; }
        public string Password { get; set; }
        public string Server { get; set; }
    }
}
