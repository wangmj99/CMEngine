using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CMEngineCore.Models
{

    /**
     * Buy Market: Stock
        {
          "orderType": "MARKET",
          "session": "NORMAL",
          "duration": "DAY",
          "orderStrategyType": "SINGLE",
          "orderLegCollection": [
            {
              "instruction": "Buy",
              "quantity": 15,
              "instrument": {
                "symbol": "XYZ",
                "assetType": "EQUITY"
              }
            }
          ]
        }

        Buy Limit: Single Option
        {
          "complexOrderStrategyType": "NONE",
          "orderType": "LIMIT",
          "session": "NORMAL",
          "price": "6.45",
          "duration": "DAY",
          "orderStrategyType": "SINGLE",
          "orderLegCollection": [
            {
              "instruction": "BUY_TO_OPEN",
              "quantity": 10,
              "instrument": {
                "symbol": "XYZ_032015C49",
                "assetType": "OPTION"
              }
            }
          ]
        }
     * */

    public class TDConstantVal
    {
        public static string OrderType_MKT = "MARKET";
        public static string OrderType_LMT = "LIMIT";

        public static string OrderSession_Normal = "NORMAL";

        public static string OrderTIF_Day = "DAY";
        public static string OrderTIF_GTC = "GOOD_TILL_CANCEL";

        public static string OrderStrategyType_Single = "SINGLE";

        public static string AssetType_EQUITY = "EQUITY";
        public static string AssetType_CASHEQL = "CASH_EQUIVALENT";
        

        public static string OrderTradeType_Buy = "BUY";
        public static string OrderTradeType_Sell = "SELL";
        public static string OrderTradeType_BuyToOpen = "BUY_TO_OPEN";

        public static string OrderStatus_Working = "WORKING";
        public static string OrderStatus_Accepted = "ACCEPTED";
        public static string OrderStatus_Queued = "QUEUED";
        public static string OrderStatus_Canceled = "CANCELED";
        public static string OrderStatus_Filled = "FILLED";
        public static string OrderStatus_AwaitReview = "AWAITING_MANUAL_REVIEW";
        public static string OrderStatus_PendingAct = "PENDING_ACTIVATION";


    }
    public class TDInstrument
    {
        public string symbol { get; set; }
        public string cusip { get; set; }
        public string assetType { get; set; }
    }

    public class OrderLegCollection
    {
        public string orderLegType { get; set; }
        public int legId { get; set; }
        public TDInstrument instrument { get; set; }
        public string instruction { get; set; }
        public string positionEffect { get; set; }
        public double quantity { get; set; }
    }

    public class TDContract
    {
        public string orderType { get; set; }
        public double price { get; set; }
        public string session { get; set; }
        //Time In Force, TIF
        public string duration { get; set; }
        public string orderStrategyType { get; set; }
        public List<OrderLegCollection> orderLegCollection { get; set; }
    }

    public class TDOrder
    {
        public string session { get; set; }
        public string duration { get; set; }
        public string orderType { get; set; }
        public string complexOrderStrategyType { get; set; }
        public double quantity { get; set; }
        public double filledQuantity { get; set; }
        public double remainingQuantity { get; set; }
        public string requestedDestination { get; set; }
        public string destinationLinkName { get; set; }
        public double price { get; set; }
        public List<OrderLegCollection> orderLegCollection { get; set; }
        public string orderStrategyType { get; set; }
        public int orderId { get; set; }
        public bool cancelable { get; set; }
        public bool editable { get; set; }
        public string status { get; set; }
        public DateTime enteredTime { get; set; }
        public string tag { get; set; }
        public int accountId { get; set; }
    }

    public class TDToken
    {
        public string access_token { get; set; }
        public string refresh_token { get; set; }
        public int expires_in { get; set; }
        public int refresh_token_expires_in { get; set; }
        public string token_type { get; set; }
    }
}
