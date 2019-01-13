using log4net;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using IBApi;
using Newtonsoft.Json;
using CMEngineCore.Models;
using System.Threading;
using System.IO;

namespace CMEngineCore
{
    public class TradeManager
    {
        public static string DataFile = "TradeManager.dat";
        private static readonly ILog Log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        [JsonIgnore]
        public IBClient IBClient { get; set; }

        [JsonIgnore]
        private EReaderMonitorSignal signal;

        [JsonIgnore]
        private object trade_locker = new object();

        [JsonIgnore]
        public bool IsInitialized { get; set; }

        [JsonIgnore]
        public static TradeManager Instance = new TradeManager();

        private TradeManager() { }

        public void Init()
        {
            if (IBClient != null)
            {
                Disconnect();
            }

            signal = new EReaderMonitorSignal();
            IBClient = new IBClient(signal);

            IBClient.OpenOrder += HandleOpenOrderMsg;
            IBClient.ExecutionDetails += HandleExecutionMsg;
            IBClient.OrderStatus += HandleOrderStatusMsg;

            IsInitialized = true;
        }

        public bool IsConnected { get { return IBClient != null && IBClient.IsConnected; } }

        public void Connect (string ip, int port, int clientID)
        {
            Log.Info("Connecting IB");
            try
            {
                IBClient.ClientSocket.eConnect(ip, port, clientID);

                var reader = new EReader(IBClient.ClientSocket, signal);
                reader.Start();

                new Thread(() => { while (IBClient.ClientSocket.IsConnected()) { signal.waitForSignal(); reader.processMsgs(); } }) { IsBackground = true }.Start();
            }
            catch(Exception ex)
            {
                Log.Error("Error on connect IB, message: " + ex.Message);
                Log.Error("Error on connect IB, StackTrace: " + ex.StackTrace);
            }
        }

        public void Disconnect()
        {
            if (IBClient.IsConnected)
            {
                try
                {
                    IBClient.ClientSocket.eDisconnect();
                    Log.Info("Disconnecting IB");
                }catch(Exception ex)
                {
                    Log.Error("Disconnect IB error. Message: " + ex.Message);
                    Log.Error(ex.StackTrace);
                }
            }
            else
            {
                Log.Info("IB is already disconnected");
            }
        }

        private void HandleOrderStatusMsg(OrderStatusMessage msg)
        {
            Log.Info(string.Format("Receive Order Status Msg. OrderID: {0}, Status: {1}", msg.OrderId, msg.Status));

            try
            {
                var parent = ParentOrderManager.Instance.GetParentOrderByChildID(msg.OrderId);
                if (parent != null)
                {
                    parent.HandleOrderStatusMsg(msg);
                }else
                {
                    Log.Error(string.Format("Cannot find associated Parent Order."));
                }

                if (IsInitialized)
                    StateManager.Save();
            }catch(Exception ex)
            {
                Log.Error("HandleOrderStatusMsg error: " + ex.Message);
                Log.Error(ex.StackTrace);
            }
        }

        private void HandleExecutionMsg(ExecutionMessage msg)
        {
            Log.Info(string.Format("Receive Execution Msg. {0}", Util.PrintExecutionMsg(msg)));

            try
            {
                var parent = ParentOrderManager.Instance.GetParentOrderByChildID(msg.Execution.OrderId);
                if (parent != null)
                {
                    parent.HandleExecutionMsg(msg);
                }
                else
                {
                    Log.Error(string.Format("Cannot find associated Parent Order."));
                }

                if (IsInitialized)
                    StateManager.Save();
            }
            catch (Exception ex)
            {
                Log.Error("HandleExecutionMsg error: " + ex.Message);
                Log.Error(ex.StackTrace);
            }
        }

        private void HandleOpenOrderMsg(OpenOrderMessage msg)
        {
            Log.Info("Receive Open Order Msg. " + string.Format("Open OrderID {0}, ClientID {1} ", msg.Order.OrderId, msg.Order.ClientId));
        }

        public TradeOrder PlaceOrder(int parentOrderID, TradeType tradeType, string symbol, double price, double qty, string exchange = null)
        {
            if (tradeType != TradeType.Buy && tradeType != TradeType.Sell) throw new Exception("Unsupported TradeType: " + tradeType);

            TradeOrder res = null;
                lock (trade_locker)
            {
                int orderID = IBClient.PlaceOrder(symbol, price, qty, tradeType, exchange);
                res = new TradeOrder();
                res.ParentOrderID = parentOrderID;
                res.OrderID = orderID;
                res.Status = TradeOrderStatus.PendingSubmit;

                ParentOrderManager.Instance.AddChildOrder(res);

                if (IsInitialized)
                    StateManager.Save();
            }

            Log.Info(string.Format("Place order ID {0}, TradeType {1}, symbol {2}, price {3}, qty {4}, exchange {5}, parentOrderID {6}", 
                res.OrderID, tradeType, symbol, price, qty, exchange, parentOrderID));

            return res;
        }

        public void CancelOrders(List<TradeOrder> orders)
        {
            lock (trade_locker)
            {
                foreach(var order in orders)
                {
                    try
                    {
                        Log.Info(string.Format("Cancel orderID: {0}", order.OrderID));
                        IBClient.ClientSocket.cancelOrder(order.OrderID);
                    }
                    catch (Exception ex)
                    {
                        Log.Error("Failed to cancel order. Error: " + ex.Message);
                    }
                }
            }
        }

        public void Save(string filename)
        {
            Util.SerializeObject<TradeManager>(Instance, filename);
        }


        public static TradeManager PopulateStates(string filename)
        {
            TradeManager res = TradeManager.Instance;
            if (File.Exists(filename))
                res = Util.DeSerializeObject<TradeManager>(filename);
            return res;
        }

        public void RequestGlobalCancle()
        {
            lock (trade_locker)
            {
                try
                {
                    Log.Info("Request Global cancel!");
                    IBClient.ClientSocket.reqGlobalCancel();
                }catch(Exception ex)
                {
                    Log.Error("Failed to request global cancel. Error: " + ex.Message);
                    Log.Error(ex.StackTrace);
                }
            }
        }
    }
}
