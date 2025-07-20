using WebHookApp.Models;

namespace WebHookApp.Logic
{
    public interface IWebSocketListener
    {
        Task StartListening(WebHookPayload payload, int ticket, double openPrice);
    }
}