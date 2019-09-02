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

        public static string OrderTradeType_Buy = "BUY";
        public static string OrderTradeType_Sell = "SELL";
        public static string OrderTradeType_BuyToOpen = "BUY_TO_OPEN";



    }
    public class Instrument
    {
        public string symbol { get; set; }
        public string assetType { get; set; }
    }

    public class OrderLegCollection
    {
        public string instruction { get; set; }
        public int quantity { get; set; }
        public Instrument instrument { get; set; }
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
}
