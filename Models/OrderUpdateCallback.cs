using System.Security.Principal;

namespace WebHookApp.Models
{
    public class OrderUpdateCallback
    {
        public string type { get; set; }
        public Guid id { get; set; }
        public Data data { get; set; }
    }

    public class Data
    {
        public Data()
        {
            orders = new List<Order>();
        }

        public double profit { get; set; }
        public int user { get; set; }
        public List<Order> orders { get; set; }
    }

    public class Order
    {
        public int ticket { get; set; }
        public double profit { get; set; }
        public double stopLoss { get; set; }
        public double takeProfit { get; set; }
    }
}
