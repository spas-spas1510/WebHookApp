using Microsoft.AspNetCore.Mvc;
using System.Transactions;
using WebHookApp.Models;

namespace WebHookApp.Logic
{
    public class PositionModifier : IPostionModifier
    {
        private string orderModifyUrl = @"https://mt5full3.mtapi.io/OrderModify";

        private readonly IConfiguration _configuration;
        public PositionModifier(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public async Task<IActionResult> ModifyPositionWithTpAndSl(string userId, int ticket, double openPrice, string action, double tp, double sl)
        {
            try
            {
                var queryParams = GetTpAndSlQueryParams(userId, ticket, openPrice, action, tp, sl);

                var url = @$"{orderModifyUrl}?{queryParams}";

                using var client = GetHttpClient();
                var response = await client.GetAsync(url);
                string body = await response.Content.ReadAsStringAsync();

                if (response.IsSuccessStatusCode)
                {
                    var text2 = $"Position update userId: {userId}";
                    Console.WriteLine(text2);
                    return new OkResult();
                }

                var text3 = $"Response: {response}; Body: {body}";
                Console.WriteLine(text3);
                return new BadRequestObjectResult(new { Error = text3 });
            }
            catch (Exception ex)
            {
                var text4 = $"Exception in Close: {ex}";
                Console.WriteLine(text4);
                return new BadRequestObjectResult(new { Error = text4 });
            }
        }

        public async Task<IActionResult> ModifyPositionTrailingStop(string userId, int ticket, string action, double openPrice, double profit, double sl)
        {
            try
            {
                var queryParams = SetSlForTralingStep(userId, ticket, action, openPrice, profit, sl);

                var url = @$"{orderModifyUrl}?{queryParams}";

                using var client = GetHttpClient();
                var response = await client.GetAsync(url);
                string body = await response.Content.ReadAsStringAsync();

                if (response.IsSuccessStatusCode)
                {
                    var text2 = $"Position update userId: {userId}";
                    Console.WriteLine(text2);
                    return new OkResult();
                }

                var text3 = $"Response: {response}; Body: {body}";
                Console.WriteLine(text3);
                return new BadRequestObjectResult(new { Error = text3 });
            }
            catch (Exception ex)
            {
                var text4 = $"Exception in Close: {ex}";
                Console.WriteLine(text4);
                return new BadRequestObjectResult(new { Error = text4 });
            }
        }

        private string SetSlForTralingStep(string userId, int ticket, string action, double openPrice, double profit, double sl)
        {
            var queryParams = new List<string>
            {
                $"id={userId}",
                $"ticket={ticket}",
                $"expirationType=Specified",
                $"takeprofit=0"
            };

            var slParam = action.ToLower() == "buy" ? openPrice + profit - sl : action.ToLower() == "sell" ? openPrice - profit + sl : 0;

            if (slParam != 0)
                queryParams.Add($"stoploss={slParam.ToString()}");            

            var result = string.Join("&", queryParams);

            return result;
        }

        private string GetTpAndSlQueryParams(string userId, int ticket, double openPrice, string action, double tp, double sl)
        {
            var queryParams = new List<string>
            {
                $"id={userId}",
                $"ticket={ticket}",
                $"expirationType=Specified",
            };

            double slParam = 0;
            if (sl != 0)
            {
                slParam = action.ToLower() == "buy" ? openPrice - sl : action.ToLower() == "sell" ? openPrice + sl : 0;
            }

            double tpParam = 0;
            if (tp != 0)
            {
                tpParam = action.ToLower() == "buy" ? openPrice + tp : action.ToLower() == "sell" ? openPrice - tp : 0;
            }

            if (slParam != 0)
                queryParams.Add($"stoploss={slParam.ToString()}");

            if (tpParam != 0)
                queryParams.Add($"takeprofit={tpParam.ToString()}");

            var result = string.Join("&", queryParams);

            return result;
        }

        public HttpClient GetHttpClient()
        {
            var client = new HttpClient();
            client.DefaultRequestHeaders.Add("ApiKey", _configuration["ApiKey"]);

            return client;
        }
    }
}
