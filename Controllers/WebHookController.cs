using Microsoft.AspNetCore.Mvc;
using WebHookApp.Logic;
using WebHookApp.Models;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace WebHookApp.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class WebHookController : ControllerBase
    {
        private IWebHookLogic webHookLogic;
        private readonly ILoginLogic loginLogic;

        public WebHookController(IWebHookLogic iWebHookLogic, ILoginLogic loginLogic)
        {
            webHookLogic = iWebHookLogic;
            this.loginLogic = loginLogic;
        }

        [HttpPost("hook")]
        public async Task<IActionResult> ReceiveWebhook([FromBody] WebHookPayload payload)
        {
            if (payload == null)
            {
                return BadRequest("Invalid payload");
            }

            if (payload.Action.ToLower() == "buy" && (payload.PositionDirection.ToLower() == "buy" || payload.PositionDirection.ToLower() == "buyandsell"))
            {
                return await webHookLogic.ExecuteMarketOrderWithApiToken(payload);
            }
            else if(payload.Action.ToLower() == "sell" && (payload.PositionDirection.ToLower() == "sell" || payload.PositionDirection.ToLower() == "buyandsell"))
            {
                return await webHookLogic.ExecuteMarketOrderWithApiToken(payload);
            }
            else if (payload.Action.ToLower() == "close")
            {
                return await webHookLogic.CloseWithApiToken(payload);
            }
            else
            {
                return await webHookLogic.CloseWithApiToken(payload);
            }

            return Ok();
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] Login login)
        {
            if (login == null)
            {
                return BadRequest("Invalid payload");
            }
            
            return await loginLogic.Login(login);
        }
    }
}
