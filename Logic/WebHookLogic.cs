using Microsoft.AspNetCore.Mvc;
using System.Text.Json;
using WebHookApp.Models;

namespace WebHookApp.Logic
{
    public class WebHookLogic : IWebHookLogic
    {
        private static Dictionary<string, long> openPostions = new Dictionary<string, long>();
        private string orderSendUrl = @"https://mt5full2.mtapi.io/OrderSend";
        private string orderCloseUrl = @"https://mt5full2.mtapi.io/OrderClose";
        
        private readonly IWebSocketListener _webSocketListener;
        private readonly IPostionModifier _postionModifier;        

        public WebHookLogic(IWebSocketListener webSocketListener, IPostionModifier postionModifier)
        {
            _webSocketListener = webSocketListener;
            _postionModifier = postionModifier;
        }

        public async Task<IActionResult> ExecuteMarketOrderWithApiToken(WebHookPayload webHookPayload)
        {
            try
            {
                var positionKey = GetPositionKey(webHookPayload.Action, webHookPayload.Symbol, webHookPayload.UserId.ToString());

                if (openPostions.ContainsKey(positionKey))
                {
                    var errorText = $"Can not open more than one position for {webHookPayload.UserId} - {webHookPayload.Symbol}";
                    Console.WriteLine(errorText);
                    return new OkObjectResult(new { Error = errorText });
                }

                //close opposite position if exists
                await CloseWithApiToken(webHookPayload, true);

                var queryParams = GetOrderOpenQueryParams(webHookPayload);
                var url = @$"{orderSendUrl}?{queryParams}";

                using var client = _postionModifier.GetHttpClient();

                var response = await client.GetAsync(url);
                string body = await response.Content.ReadAsStringAsync();

                if (response.IsSuccessStatusCode)
                {
                    var postionResponse = JsonSerializer.Deserialize<PositionResponse>(body);
                    if (postionResponse != null && postionResponse.ticket > 0)
                    {
                        openPostions.Add(positionKey, postionResponse.ticket);

                        if (webHookPayload.TrailingDistanceDollars > 0 && webHookPayload.TrailingStepDollars > 0)
                        {
                            await _postionModifier.ModifyPositionWithTpAndSl(webHookPayload.UserId.ToString(), postionResponse.ticket, postionResponse.openPrice, webHookPayload.Action, 0, webHookPayload.TrailingDistanceDollars);
                            await _webSocketListener.StartListening(webHookPayload, postionResponse.ticket, postionResponse.openPrice);
                        }
                        else if (webHookPayload.TakeProfitDollars > 0 && webHookPayload.StopLossDollars > 0)
                        {
                            await _postionModifier.ModifyPositionWithTpAndSl(webHookPayload.UserId.ToString(), postionResponse.ticket, postionResponse.openPrice, webHookPayload.Action, webHookPayload.TakeProfitDollars, webHookPayload.StopLossDollars);
                        }                        

                        Console.WriteLine($"Position open userId: {webHookPayload.UserId}");
                        return new OkResult();
                    }

                    Console.WriteLine(body);
                    return new OkObjectResult(new { Message = body });
                }

                var text1 = $"Response: {response}; Body: {body}";
                Console.WriteLine(text1);
                return new BadRequestObjectResult(new { Error = text1 });
            }
            catch (Exception ex)
            {
                var text2 = $"Exception in ExecuteMarketOrder: {ex}";
                Console.WriteLine(text2);
                return new BadRequestObjectResult(new { Error = text2 });
            }
        }

        public async Task<IActionResult> CloseWithApiToken(WebHookPayload webHookPayload, bool isOppositeOperation = false)
        {
            try
            {
                var actionText = isOppositeOperation ? GetOppositeAction(webHookPayload.Action) : webHookPayload.Action;
                var closePositionKey = GetClosePositionKey(webHookPayload.Symbol, webHookPayload.UserId.ToString());
                long ticket = 0;

                var openTradePositions = openPostions.Keys.Where(x => x.EndsWith(closePositionKey));
                string text3 = "";

                foreach (var openTradePosition in openTradePositions)
                {
                    if (!openPostions.TryGetValue(openTradePosition, out ticket))
                    {
                        var text1 = $"Open position does not exist!";
                        Console.WriteLine(text1);
                        return new OkObjectResult(text1);
                    }

                    var queryParams = GetOrderCloseQueryParams(webHookPayload.UserId.ToString(), ticket.ToString());

                    var url = @$"{orderCloseUrl}?{queryParams}";

                    using var client = _postionModifier.GetHttpClient();
                    var response = await client.GetAsync(url);
                    string body = await response.Content.ReadAsStringAsync();

                    if (response.IsSuccessStatusCode)
                    {
                        openPostions.Remove(openTradePosition);
                        var text2 = $"Position close userId: {webHookPayload.UserId}";
                        Console.WriteLine(text2);
                        return new OkResult();
                    }

                    text3 += $"Response: {response}; Body: {body}";
                    Console.WriteLine(text3);
                }

                return new BadRequestObjectResult(new { Error = text3 });
            }
            catch (Exception ex)
            {
                var text4 = $"Exception in Close: {ex}";
                Console.WriteLine(text4);
                return new BadRequestObjectResult(new { Error = text4 });
            }
        }

        private string GetOrderCloseQueryParams(string userId, string ticket)
        {
            var queryParams = new List<string>
            {
                $"id={userId }",
                $"ticket={ticket}"
            };

            var result = string.Join("&", queryParams);

            return result;
        }

        private string GetOrderOpenQueryParams(WebHookPayload payload)
        {
            var queryParams = new List<string>
            {
                $"id={payload.UserId.ToString()}",
                $"symbol={payload.Symbol}",
                $"operation={GetAction(payload.Action)}",
                $"volume={payload.Volume.ToString()}",
                $"expirationType=Specified",
                $"placedType=Manually",
                $"comment={payload.Strategy}"
            };

            var result = string.Join("&", queryParams);

            return result;
        }        

        private string GetAction(string actionText)
        {
            switch (actionText.ToLower())
            {
                case "buy": return "Buy";
                case "sell": return "Sell";
                case "close": return "Close";
                default: throw new Exception("Action not recognized " + actionText);
            };
        }

        private string GetOppositeAction(string actionText)
        {
            switch (actionText.ToLower())
            {
                case "buy": return "Sell";
                case "sell": return "Buy";
                default: throw new Exception("Action not recognized " + actionText);
            };
        }

        private string GetPositionKey(string action, string symbol, string userId)
        {
            return $"{action.ToLower()}_{symbol.ToLower()}_{userId.ToLower()}";
        }

        private string GetClosePositionKey(string symbol, string userId)
        {
            return $"{symbol.ToLower()}_{userId.ToLower()}";
        }
    }
}
