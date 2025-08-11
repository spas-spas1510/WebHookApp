using Microsoft.AspNetCore.Mvc;
using WebHookApp.Models;

namespace WebHookApp.Logic
{
    public interface IWebHookLogic
    {
        Task<IActionResult> ExecuteMarketOrderWithApiToken(WebHookPayload webHookPayload);
        Task<bool> CloseWithApiToken(WebHookPayload webHookPayload, bool isOppositeOperation = false);
    }
}