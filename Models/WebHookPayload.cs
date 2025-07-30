namespace WebHookApp.Models
{
    public class WebHookPayload
    {
        public string Action { get; set; }
        public string Symbol { get; set; }
        public double Volume { get; set; }        
        public double StopLossDollars { get; set; }
        public double TakeProfitDollars { get; set; }
        public double TrailingDistanceDollars { get; set; }
        public double TrailingStepDollars { get; set; }
        public DateTime TimeStamp { get; set; }
        public string Strategy { get; set; }
        public Guid UserId { get; set; }
        public string BotToken { get; set; }
        public string PositionDirection { get; set; }
    }
}
