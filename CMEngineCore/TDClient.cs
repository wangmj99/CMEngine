using CMEngineCore.Models;
using IBApi;
using log4net;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Configuration;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace CMEngineCore
{
    public class TDClient
    {
        private static readonly ILog Log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);



        string refreshToken = string.Empty;

        private static string OrderURL = @"https://api.tdameritrade.com/v1/accounts/{0}/orders";
        private static string CancelOrderURL = @"https://api.tdameritrade.com/v1/accounts/{0}/orders/{1}";

        public string accountId = string.Empty;

        public static TDClient Instance = new TDClient();

        private TDClient()
        {
            accountId = ConfigurationManager.AppSettings["TDAccountID"];
            refreshToken = ConfigurationManager.AppSettings["TDToken"];
            OrderURL = string.Format(OrderURL, accountId);
            CancelOrderURL = string.Format(OrderURL, accountId);
            webClient = new WebClient();
        }

        private WebClient webClient;

        public event Action<ExecutionMessage> ExecutionDetails;

        public void execDetails(int reqId, Contract contract, Execution execution)
        {
            var tmp = ExecutionDetails;
            if (tmp != null)
                tmp(new ExecutionMessage(reqId, contract, execution));
        }

        public event Action<OrderStatusMessage> OrderStatus;

        public void orderStatus(int orderId, string status, double filled, double remaining, double avgFillPrice, int permId, int parentId, double lastFillPrice, int clientId, string whyHeld, double mktCapPrice)
        {
            if (OrderStatus != null)
                OrderStatus(new OrderStatusMessage(orderId, status, filled, remaining, avgFillPrice, permId, parentId, lastFillPrice, clientId, whyHeld, mktCapPrice));
        }

        public bool Connect()
        {
            return true;
        }

        public int PlaceOrder(string symbol, double price, double qty, TradeType tradeType, string exchange = null, OrderType orderType = OrderType.LMT)
        {
            int orderID = -1;
            bool isMarketOrder = (orderType == OrderType.MKT);
            try
            {
                TDInstrument inst = new TDInstrument();
                inst.assetType = TDConstantVal.AssetType_EQUITY;
                inst.symbol = symbol;

                OrderLegCollection col = new OrderLegCollection();

                if (tradeType == TradeType.Buy)
                    col.instruction = TDConstantVal.OrderTradeType_Buy;
                else if (tradeType == TradeType.Sell)
                    col.instruction = TDConstantVal.OrderTradeType_Sell;
                else
                {
                    Log.Error("Unsupported trade type: " + tradeType.ToString());
                }
                col.quantity = (int)qty;  //truncate decimal
                col.instrument = inst;


                TDContract contract = new TDContract();
                contract.duration = TDConstantVal.OrderTIF_Day;
                contract.price = price;
                contract.orderType = isMarketOrder? TDConstantVal.OrderType_MKT: TDConstantVal.OrderType_LMT;
                contract.session = TDConstantVal.OrderSession_Normal;
                contract.orderStrategyType = TDConstantVal.OrderStrategyType_Single;

                contract.orderLegCollection = new List<OrderLegCollection>() { col };

                string msg = JsonConvert.SerializeObject(contract);

                var res  = SendMessage(OrderURL, "POST", msg);

                orderID = ParseOrderID(res.Item1);

            }catch(Exception ex)
            {
                Log.Error(string.Format("Error placing order: {0}", ex.Message));
                throw ex;
            }

            return orderID;
        }

        public void CancelOrder(int orderId)
        {
            try
            {
                string url = string.Format("{0}/{1}", CancelOrderURL,  orderId);
                SendMessage(url, "DELETE", string.Empty);
            }
            catch (Exception ex)
            {
                Log.Error(string.Format("Error cancel orderID: {0}, error: {1}", orderId, ex.Message));
                throw ex;
            }
        }

        public List<TDOrder> GetOpenOrders()
        {
            List<string> stats = new List<string>() {
                TDConstantVal.OrderStatus_Queued,
                TDConstantVal.OrderStatus_Working,
                TDConstantVal.OrderStatus_Accepted,
                TDConstantVal.OrderStatus_AwaitReview,
                TDConstantVal.OrderStatus_PendingAct
            };
            List<TDOrder> res = new List<TDOrder>();
            string accessToken = GetAccessToken();
            
            foreach (string s in stats)
            {
                try
                {
                    webClient = new WebClient();
                    webClient.Headers[HttpRequestHeader.Authorization] = string.Format("Bearer {0}", accessToken);
                    webClient.Headers[HttpRequestHeader.UserAgent] = "Mozilla/5.0 (Windows NT 6.1; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/58.0.3029.110 Safari/537.36";
                    webClient.QueryString.Add("status", s);
                    string orderStr = webClient.DownloadString(OrderURL);
                    res.AddRange(JsonConvert.DeserializeObject<List<TDOrder>>(orderStr));
                    webClient.Dispose();
                }catch(Exception ex)
                {
                    Log.Error(string.Format("Error getting open orders. status: {0}, error: {1}", s, ex.ToString()));
                }
            }

            return res;
        }

        public void RequestGlobalCancel()
        {
            List<TDOrder> orders = GetOpenOrders();
            if (orders.Count > 0)
            {
                foreach(var order in orders)
                {
                    try
                    {
                        CancelOrder(order.orderId);
                    }
                    catch (Exception ex)
                    {
                        Log.Error(string.Format("Error cancel open order {0}, error: {0}", order.orderId, ex.ToString()));
                    }
                }
            }
        }

        private Tuple<WebHeaderCollection, string> SendMessage(string url, string method, string msg)
        {
            string res = string.Empty;
            WebHeaderCollection header = null;
            try
            {
                string accessToken = GetAccessToken();
                webClient = new WebClient();
                webClient.Headers[HttpRequestHeader.Authorization] = string.Format("Bearer {0}", accessToken);
                webClient.Headers[HttpRequestHeader.UserAgent] = "Mozilla/5.0 (Windows NT 6.1; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/58.0.3029.110 Safari/537.36";
                webClient.Headers[HttpRequestHeader.ContentType] = "application/json";
                res = webClient.UploadString(url, method, msg);

                header = webClient.ResponseHeaders;
            }catch(Exception ex)
            {
                Log.Error(ex.Message);
                throw ex;
            }

            return new Tuple<WebHeaderCollection, string>(header, res);
        }

        private static Regex placeOrderPattern = new Regex("orders/\\d+");
        private static int  ParseOrderID(WebHeaderCollection header)
        {                
            //response header Locatin string: https://api.tdameritrade.com/v1/accounts/488490405/orders/675583148
            //string str = webClient.ResponseHeaders[HttpResponseHeader.Location];

            int res = -1;
            string str = header[HttpResponseHeader.Location];
            string val = placeOrderPattern.Match(str).Value;

            if(!string.IsNullOrWhiteSpace(val))
            {
                res = int.Parse(val.Split(new string[] { "/" }, StringSplitOptions.RemoveEmptyEntries)[1]);
            }else
            {
                Log.Error(string.Format("invalid orderID, header string: {0}", str));
                throw new Exception("invalid orderID");
            }

            return res;
        }

        private const string AccessTokenUrl = "https://api.tdameritrade.com/v1/oauth2/token";
        public string GetAccessToken()
        {
            string res = string.Empty;

            var reqparm = new NameValueCollection();

            reqparm["grant_type"] = "refresh_token";
            reqparm["refresh_token"] = refreshToken;
            reqparm["access_type"] = string.Empty;
            reqparm["code"] = string.Empty;
            reqparm["client_id"] = ConfigurationManager.AppSettings["TDAppID"];
            reqparm["redirect_uri"] = string.Empty;

            webClient = new WebClient();
            webClient.Headers[HttpRequestHeader.UserAgent] = "Mozilla/5.0 (Windows NT 6.1; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/58.0.3029.110 Safari/537.36";
            webClient.Headers[HttpRequestHeader.ContentType] = "application/x-www-form-urlencoded";

            var str = SetMessageWithParam(webClient, AccessTokenUrl, "POST", reqparm);

             res = JsonConvert.DeserializeObject<TDToken>(str).access_token;

            return res;
        }

        private string SetMessageWithParam(WebClient webClient, string url, string method, NameValueCollection reqparm)
        {
            var responsebytes = webClient.UploadValues(url, method, reqparm);
            var str = Encoding.UTF8.GetString(responsebytes);
            return str;
        }
    }
}
