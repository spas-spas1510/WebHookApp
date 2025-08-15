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

            payload.Action = payload.InvertDirection ? GetOppositeDirection(payload.Action) : payload.Action;
            payload.Comment = payload.InvertDirection ? GetOppositeDirection(payload.Comment) : payload.Comment;

            if (HasBuyCriteria(payload))
            {
                return await webHookLogic.ExecuteMarketOrderWithApiToken(payload);
            }
            else if (HasSellCriteria(payload))
            {
                return await webHookLogic.ExecuteMarketOrderWithApiToken(payload);            
            }
            else
            {
                await webHookLogic.CloseWithApiToken(payload);
                return Ok();
            }
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

        private bool HasBuyCriteria(WebHookPayload payload)
        {
            return payload.Action.ToLower() == "buy" &&
                (payload.PositionDirection.ToLower() == "buy" || payload.PositionDirection.ToLower() == "buyandsell") &&
                (payload.Comment.ToLower() == "buy" || payload.Comment.ToLower() == "long");
        }

        private bool HasSellCriteria(WebHookPayload payload)
        {
            return payload.Action.ToLower() == "sell" && 
                (payload.PositionDirection.ToLower() == "sell" || payload.PositionDirection.ToLower() == "buyandsell") &&
                (payload.Comment.ToLower() == "sell" || payload.Comment.ToLower() == "short");
        }

        private string GetOppositeDirection(string direction)
        {
            switch (direction.ToLower())
            {
                case "buy": return "sell";
                case "sell": return "buy";
                case "short": return "long";
                case "long": return "short";
                default: return "close";
            }
        }
    }
}
