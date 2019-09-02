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
        public static string OrderTypeMKT = "MARKET";
        public static string OrderTypeLMT = "LIMIT";
        public static string OrderSessionNormal = "NORMAL";
        public static string OrderTIFDay = "DAY";
        public static string OrderTIFGTC = "GOOD_TILL_CANCEL";
        public static string OrderStrategyTypeSingle = "SINGLE";
        public static string AssetTypeEQUITY = "EQUITY";
        public static string OrderTradeTypeBuy = "BUY";
        public static string OrderTradeTypeSell = "SELL";

        

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
        public string duration { get; set; }
        public string orderStrategyType { get; set; }
        public List<OrderLegCollection> orderLegCollection { get; set; }
    }
}
