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
        public WebHookController(IWebHookLogic iWebHookLogic)
        {
            webHookLogic = iWebHookLogic;
        }

        [HttpPost("hook")]
        public async Task<IActionResult> ReceiveWebhook([FromBody] WebHookPayload payload)
        {
            if (payload == null)
            {
                return BadRequest("Invalid payload");
            }

            if (payload.Action.ToLower() == "buy" || payload.Action.ToLower() == "sell")
            {
                return await webHookLogic.ExecuteMarketOrderWithApiToken(payload);
            }
            else if (payload.Action.ToLower() == "close")
            {                                
                return await webHookLogic.CloseWithApiToken(payload);
            }

            return Ok();
        }
    }
}
