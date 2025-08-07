using Microsoft.AspNetCore.Mvc;
using System.Text.Json;
using WebHookApp.Models;

namespace WebHookApp.Logic
{
    public class LoginLogic : ILoginLogic
    {
        private const string connectExUrl = "https://mt5full3.mtapi.io/ConnectEx";
        private readonly IPostionModifier _postionModifier;

        public LoginLogic(IPostionModifier postionModifier)
        {
            _postionModifier = postionModifier;
        }
        public async Task<IActionResult> Login(Login login)
        {
            var queryParams = GetOrderOpenQueryParams(login);
            var url = @$"{connectExUrl}?{queryParams}";

            using var client = _postionModifier.GetHttpClient();

            var response = await client.GetAsync(url);
            string body = await response.Content.ReadAsStringAsync();

            if (response.IsSuccessStatusCode)
            {
                return new OkObjectResult(body);
            }

            return new BadRequestObjectResult(response);
        }

        private string GetOrderOpenQueryParams(Login login)
        {
            var queryParams = new List<string>
            {
                $"user={login.AccountId.ToString()}",
                $"password={Uri.EscapeDataString(login.Password)}",
                $"server={login.Server}",
                $"id={login.Id}",
                $"connectTimeoutSeconds=60",
                $"connectTimeoutClusterMemberSeconds=20"
            };

            var result = string.Join("&", queryParams);

            return result;
        }
    }
}
