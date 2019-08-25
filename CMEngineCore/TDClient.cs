using CMEngineCore.Models;
using IBApi;
using log4net;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace CMEngineCore
{
    public class TDClient
    {
        private static readonly ILog Log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        public static TDClient Instance = new TDClient();

        string token = string.Empty;

        private TDClient()
        {
            webClient = new WebClient();
            webClient.Headers.Add("User-Agent", "Mozilla/5.0 (Windows NT 6.1; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/58.0.3029.110 Safari/537.36");
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
            return 1;
        }

        public void CancelOrder(int orderId)
        {

        }

        public void RequestGlobalCancel()
        {

        }

        private string SendMessage(string url, string method, string msg)
        {
            string res = string.Empty;
            try
            {
                res = webClient.UploadString(url, method, msg);
            }catch(Exception ex)
            {
                Log.Error(ex.Message);
                throw ex;
            }

            return res;
        }
    }
}
