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
        public const string OrderStatus_Rejected = "REJECTED";
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

        public TDContract()
        {
            orderLegCollection = new List<OrderLegCollection>();
        }
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

        public TDOrderActivityCollection()
        {
            executionLegs = new List<TDExecutionLeg>();
        }
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

        public TDOrder()
        {
            orderLegCollection = new List<OrderLegCollection>();
            orderActivityCollection = new List<TDOrderActivityCollection>();
        }
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

    
    //Order:
{
"session": "'NORMAL' or 'AM' or 'PM' or 'SEAMLESS'",
"duration": "'DAY' or 'GOOD_TILL_CANCEL' or 'FILL_OR_KILL'",
"orderType": "'MARKET' or 'LIMIT' or 'STOP' or 'STOP_LIMIT' or 'TRAILING_STOP' or 'MARKET_ON_CLOSE' or 'EXERCISE' or 'TRAILING_STOP_LIMIT' or 'NET_DEBIT' or 'NET_CREDIT' or 'NET_ZERO'",
"cancelTime": {
"date": "string",
"shortFormat": false
},
"complexOrderStrategyType": "'NONE' or 'COVERED' or 'VERTICAL' or 'BACK_RATIO' or 'CALENDAR' or 'DIAGONAL' or 'STRADDLE' or 'STRANGLE' or 'COLLAR_SYNTHETIC' or 'BUTTERFLY' or 'CONDOR' or 'IRON_CONDOR' or 'VERTICAL_ROLL' or 'COLLAR_WITH_STOCK' or 'DOUBLE_DIAGONAL' or 'UNBALANCED_BUTTERFLY' or 'UNBALANCED_CONDOR' or 'UNBALANCED_IRON_CONDOR' or 'UNBALANCED_VERTICAL_ROLL' or 'CUSTOM'",
"quantity": 0,
"filledQuantity": 0,
"remainingQuantity": 0,
"requestedDestination": "'INET' or 'ECN_ARCA' or 'CBOE' or 'AMEX' or 'PHLX' or 'ISE' or 'BOX' or 'NYSE' or 'NASDAQ' or 'BATS' or 'C2' or 'AUTO'",
"destinationLinkName": "string",
"releaseTime": "string",
"stopPrice": 0,
"stopPriceLinkBasis": "'MANUAL' or 'BASE' or 'TRIGGER' or 'LAST' or 'BID' or 'ASK' or 'ASK_BID' or 'MARK' or 'AVERAGE'",
"stopPriceLinkType": "'VALUE' or 'PERCENT' or 'TICK'",
"stopPriceOffset": 0,
"stopType": "'STANDARD' or 'BID' or 'ASK' or 'LAST' or 'MARK'",
"priceLinkBasis": "'MANUAL' or 'BASE' or 'TRIGGER' or 'LAST' or 'BID' or 'ASK' or 'ASK_BID' or 'MARK' or 'AVERAGE'",
"priceLinkType": "'VALUE' or 'PERCENT' or 'TICK'",
"price": 0,
"taxLotMethod": "'FIFO' or 'LIFO' or 'HIGH_COST' or 'LOW_COST' or 'AVERAGE_COST' or 'SPECIFIC_LOT'",
"orderLegCollection": [
{
  "orderLegType": "'EQUITY' or 'OPTION' or 'INDEX' or 'MUTUAL_FUND' or 'CASH_EQUIVALENT' or 'FIXED_INCOME' or 'CURRENCY'",
  "legId": 0,
  "instrument": "The type <Instrument> has the following subclasses [Option, MutualFund, CashEquivalent, Equity, FixedIncome] descriptions are listed below\"",
  "instruction": "'BUY' or 'SELL' or 'BUY_TO_COVER' or 'SELL_SHORT' or 'BUY_TO_OPEN' or 'BUY_TO_CLOSE' or 'SELL_TO_OPEN' or 'SELL_TO_CLOSE' or 'EXCHANGE'",
  "positionEffect": "'OPENING' or 'CLOSING' or 'AUTOMATIC'",
  "quantity": 0,
  "quantityType": "'ALL_SHARES' or 'DOLLARS' or 'SHARES'"
}
],
"activationPrice": 0,
"specialInstruction": "'ALL_OR_NONE' or 'DO_NOT_REDUCE' or 'ALL_OR_NONE_DO_NOT_REDUCE'",
"orderStrategyType": "'SINGLE' or 'OCO' or 'TRIGGER'",
"orderId": 0,
"cancelable": false,
"editable": false,
"status": "'AWAITING_PARENT_ORDER' or 'AWAITING_CONDITION' or 'AWAITING_MANUAL_REVIEW' or 'ACCEPTED' or 'AWAITING_UR_OUT' or 'PENDING_ACTIVATION' or 'QUEUED' or 'WORKING' or 'REJECTED' or 'PENDING_CANCEL' or 'CANCELED' or 'PENDING_REPLACE' or 'REPLACED' or 'FILLED' or 'EXPIRED'",
"enteredTime": "string",
"closeTime": "string",
"tag": "string",
"accountId": 0,
"orderActivityCollection": [
"The type <OrderActivity> has the following subclasses [Execution] descriptions are listed below"
],
"replacingOrderCollection": [
{}
],
"childOrderStrategies": [
{}
],
"statusDescription": "string"
}

//The class <Instrument> has the 
//following subclasses: 
//-Option
//-MutualFund
//-CashEquivalent
//-Equity
//-FixedIncome
//JSON for each are listed below: 

//Option:
{
"assetType": "'EQUITY' or 'OPTION' or 'INDEX' or 'MUTUAL_FUND' or 'CASH_EQUIVALENT' or 'FIXED_INCOME' or 'CURRENCY'",
"cusip": "string",
"symbol": "string",
"description": "string",
"type": "'VANILLA' or 'BINARY' or 'BARRIER'",
"putCall": "'PUT' or 'CALL'",
"underlyingSymbol": "string",
"optionMultiplier": 0,
"optionDeliverables": [
{
  "symbol": "string",
  "deliverableUnits": 0,
  "currencyType": "'USD' or 'CAD' or 'EUR' or 'JPY'",
  "assetType": "'EQUITY' or 'OPTION' or 'INDEX' or 'MUTUAL_FUND' or 'CASH_EQUIVALENT' or 'FIXED_INCOME' or 'CURRENCY'"
}
]
}

//OR

//MutualFund:
{
"assetType": "'EQUITY' or 'OPTION' or 'INDEX' or 'MUTUAL_FUND' or 'CASH_EQUIVALENT' or 'FIXED_INCOME' or 'CURRENCY'",
"cusip": "string",
"symbol": "string",
"description": "string",
"type": "'NOT_APPLICABLE' or 'OPEN_END_NON_TAXABLE' or 'OPEN_END_TAXABLE' or 'NO_LOAD_NON_TAXABLE' or 'NO_LOAD_TAXABLE'"
}

//OR

//CashEquivalent:
{
"assetType": "'EQUITY' or 'OPTION' or 'INDEX' or 'MUTUAL_FUND' or 'CASH_EQUIVALENT' or 'FIXED_INCOME' or 'CURRENCY'",
"cusip": "string",
"symbol": "string",
"description": "string",
"type": "'SAVINGS' or 'MONEY_MARKET_FUND'"
}

//OR

//Equity:
{
"assetType": "'EQUITY' or 'OPTION' or 'INDEX' or 'MUTUAL_FUND' or 'CASH_EQUIVALENT' or 'FIXED_INCOME' or 'CURRENCY'",
"cusip": "string",
"symbol": "string",
"description": "string"
}

//OR

//FixedIncome:
{
"assetType": "'EQUITY' or 'OPTION' or 'INDEX' or 'MUTUAL_FUND' or 'CASH_EQUIVALENT' or 'FIXED_INCOME' or 'CURRENCY'",
"cusip": "string",
"symbol": "string",
"description": "string",
"maturityDate": "string",
"variableRate": 0,
"factor": 0
}

//The class <OrderActivity> has the 
//following subclasses: 
//-Execution
//JSON for each are listed below: 

//Execution:
{
"activityType": "'EXECUTION' or 'ORDER_ACTION'",
"executionType": "'FILL'",
"quantity": 0,
"orderRemainingQuantity": 0,
"executionLegs": [
{
  "legId": 0,
  "quantity": 0,
  "mismarkedQuantity": 0,
  "price": 0,
  "time": "string"
}
]
}
 */
}


