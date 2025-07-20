using Microsoft.AspNetCore.Mvc;
using WebHookApp.Models;

namespace WebHookApp.Logic
{
    public interface IPostionModifier
    {
        Task<IActionResult> ModifyPositionWithTpAndSl(string userId, int ticket, double openPrice, string action, double tp, double sl);
        Task<IActionResult> ModifyPositionTrailingStop(string userId, int ticket, string action, double openPrice, double profit, double sl);
        HttpClient GetHttpClient();
    }
}