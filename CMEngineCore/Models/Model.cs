using IBApi;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CMEngineCore.Models
{
    public enum TradeType
    {
        Buy,
        Sell,
        BuyToCover,
        SellShort,
        Hold
    }

    public enum OrderType
    {
        MKT,
        LMT
    }

    public enum TradeOrderStatus
    {
        Submitted,
        Filled,
        Cancelled,
        PendingCxl,
        PendingSubmit
    }

    public class IBExchanges
    {
        public static string NYSE = "NYSE";
        public static string NASDAQ = "ISLAND";
    }

    public class Constant
    {
        public const string ExecutionBuy = "BOT";
        public const string ExecutionSell = "SLD";

        public const string OrderCancelled = "Cancelled";
        public const string OrderApiCancelled = "ApiCancelled";
        public const string OrderSubmitted = "Submitted";
        public const string OrderFilled = "Filled";

    }

    public class TradeOrder
    {
        public int OrderID { get; set; }
        public int ParentOrderID { get; set; }
        //public Order Order { get; set; }
        public TradeOrderStatus Status { get; set; }
        public string Notes { get; set; }
        public string Side { get; set; }
        public double Price { get; set; }
        
    }

    public enum Broker
    {
        IB,
        TD
    }
}
