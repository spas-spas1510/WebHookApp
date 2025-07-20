using Newtonsoft.Json;
using System.Net.WebSockets;
using System.Text;
using WebHookApp.Models;

namespace WebHookApp.Logic
{
    public class WebSocketListener : IWebSocketListener
    {
        private ClientWebSocket _webSocket;
        private readonly IPostionModifier _positionModifier;

        public WebSocketListener(IPostionModifier positionModifier)
        {
            _positionModifier = positionModifier;
        }

        public async Task StartListening(WebHookPayload payload, int ticket, double openPrice)
        {
            var uri = $"wss://mt5full2.mtapi.io/OnOrderProfit?id={payload.UserId}";
            _webSocket = new ClientWebSocket();

            try
            {
                await _webSocket.ConnectAsync(new Uri(uri), CancellationToken.None);

                Console.WriteLine("Connected to WebSocket server");
                _ = Task.Run(() => ReceiveMessagesAsync(ticket, payload.TrailingDistanceDollars, payload.TrailingStepDollars, payload.Action, openPrice));
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Connection error: {ex.Message}");
                throw;
            }
        }

        private async Task ReceiveMessagesAsync(int ticket, double trailingDistance, double trailingStep, string action, double openPrice)
        {
            var buffer = new byte[1024 * 14];

            while (_webSocket?.State == WebSocketState.Open)
            {
                try
                {
                    var result = await _webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);

                    if (result.MessageType == WebSocketMessageType.Close)
                    {
                        await _webSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Closed by server", CancellationToken.None);
                        break;
                    }

                    var message = Encoding.UTF8.GetString(buffer, 0, result.Count);
                    HandleWebSocketMessage(message, ticket, trailingDistance, trailingStep, action, openPrice);

                    Thread.Sleep(1000);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Receive error: {ex.Message}");
                    break;
                }
            }
        }

        private void HandleWebSocketMessage(string jsonMessage, int ticket, double trailingDistance, double trailingStep, string action, double openPrice)
        {
            try
            {
                var message = JsonConvert.DeserializeObject<OrderUpdateCallback>(jsonMessage);

                if (message != null && message.type == "OrderProfit")
                {
                    var openPositions = message.data.orders.Where(x => x.ticket == ticket).ToList();

                    foreach (var openPosition in openPositions)
                    {
                        if (HasTrailingStep(openPosition.stopLoss, openPosition.profit, trailingStep, trailingDistance, action, openPrice))
                        {
                            _positionModifier.ModifyPositionTrailingStop(message.id.ToString(), ticket, action, openPrice, openPosition.profit, trailingDistance);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Message processing error: {ex.Message}");
            }
        }

        private bool HasTrailingStep(double stopLossPrice, double profit, double trailingStep, double trailingDistance, string action, double openPrice)
        {
            switch (action)
            {
                case "buy":
                    {
                        return openPrice + profit - trailingDistance - stopLossPrice > trailingStep;
                    }
                case "sell":
                    {
                        return (openPrice - profit + trailingDistance - stopLossPrice) * -1 > trailingStep;
                    }
                default:
                    {
                        return false;
                    }
            }
        }
    }
}
