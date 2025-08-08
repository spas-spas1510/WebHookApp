using Microsoft.AspNetCore.Mvc;
using System.Text.Json;
using WebHookApp.Models;

namespace WebHookApp.Logic
{
    public class LoginLogic : ILoginLogic
    {
        private readonly string connectExUrl;
        private readonly IPostionModifier _postionModifier;

        public LoginLogic(IPostionModifier postionModifier, IConfiguration configuration)
        {
            _postionModifier = postionModifier;
            connectExUrl = $"{configuration["ApiUrl"]}/ConnectEx";
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
                $"connectTimeoutSeconds=60",
                $"connectTimeoutClusterMemberSeconds=20"
            };

            if(login.Id != null && login.Id != Guid.Empty)
            {
                queryParams.Add($"id={login.Id}");
            }

            var result = string.Join("&", queryParams);

            return result;
        }
    }
}
