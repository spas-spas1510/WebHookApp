using Microsoft.AspNetCore.Mvc;
using WebHookApp.Models;

namespace WebHookApp.Logic
{
    public interface ILoginLogic
    {
        Task<IActionResult> Login(Login login);
    }
}