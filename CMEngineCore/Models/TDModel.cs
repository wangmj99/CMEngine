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
        public const string OrderType_MKT = "MARKET";
        public const string OrderType_LMT = "LIMIT";

        public const string OrderSession_Normal = "NORMAL";

        public const string OrderTIF_Day = "DAY";
        public const string OrderTIF_GTC = "GOOD_TILL_CANCEL";

        public const string OrderStrategyType_Single = "SINGLE";

        public const string AssetType_EQUITY = "EQUITY";
        public const string AssetType_CASHEQL = "CASH_EQUIVALENT";


        public const string OrderTradeType_Buy = "BUY";
        public const string OrderTradeType_Sell = "SELL";
        public const string OrderTradeType_BuyToOpen = "BUY_TO_OPEN";

        public const string OrderStatus_Working = "WORKING";
        public const string OrderStatus_Accepted = "ACCEPTED";
        public const string OrderStatus_Queued = "QUEUED";
        public const string OrderStatus_Canceled = "CANCELED";
        public const string OrderStatus_Filled = "FILLED";
        public const string OrderStatus_AwaitReview = "AWAITING_MANUAL_REVIEW";
        public const string OrderStatus_PendingAct = "PENDING_ACTIVATION";


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

    public class TDExecutionLeg
    {
        public int legId { get; set; }
        public double quantity { get; set; }
        public double mismarkedQuantity { get; set; }
        public double price { get; set; }
        public DateTime time { get; set; }
        //following field need to be populated
        //public int orderId { get; set; }
        //public string side { get; set; }

    }


    public class TDOrderActivityCollection
    {
        public string activityType { get; set; }
        public string executionType { get; set; }
        public double quantity { get; set; }
        public double orderRemainingQuantity { get; set; }
        public List<TDExecutionLeg> executionLegs { get; set; }
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
        public DateTime closeTime { get; set; }
        public string tag { get; set; }
        public int accountId { get; set; }
        public List<TDOrderActivityCollection> orderActivityCollection { get; set; }
    }

    public class TDToken
    {
        public string access_token { get; set; }
        public string refresh_token { get; set; }
        public int expires_in { get; set; }
        public int refresh_token_expires_in { get; set; }
        public string token_type { get; set; }
    }

    /** 
     * TDOrder
         {
          "session": "NORMAL",
          "duration": "DAY",
          "orderType": "LIMIT",
          "complexOrderStrategyType": "NONE",
          "quantity": 300,
          "filledQuantity": 300,
          "remainingQuantity": 0,
          "requestedDestination": "AUTO",
          "destinationLinkName": "NITE",
          "price": 29.45,
          "orderLegCollection": [
            {
              "orderLegType": "EQUITY",
              "legId": 1,
              "instrument": {
                "assetType": "EQUITY",
                "cusip": "92189F106",
                "symbol": "GDX"
              },
              "instruction": "SELL",
              "positionEffect": "CLOSING",
              "quantity": 300
            }
          ],
          "orderStrategyType": "SINGLE",
          "orderId": 657267395,
          "cancelable": false,
          "editable": false,
          "status": "FILLED",
          "enteredTime": "2019-08-09T18:39:51+0000",
          "closeTime": "2019-08-09T18:42:17+0000",
          "tag": "WEB_GRID",
          "accountId": 488490405,
          "orderActivityCollection": [
            {
              "activityType": "EXECUTION",
              "executionType": "FILL",
              "quantity": 200,
              "orderRemainingQuantity": 100,
              "executionLegs": [
                {
                  "legId": 1,
                  "quantity": 200,
                  "mismarkedQuantity": 0,
                  "price": 29.45,
                  "time": "2019-08-09T18:39:59+0000"
                }
              ]
            },
            {
              "activityType": "EXECUTION",
              "executionType": "FILL",
              "quantity": 100,
              "orderRemainingQuantity": 0,
              "executionLegs": [
                {
                  "legId": 1,
                  "quantity": 100,
                  "mismarkedQuantity": 0,
                  "price": 29.45,
                  "time": "2019-08-09T18:42:17+0000"
                }
              ]
            }
          ]
        }
     */
}
    

