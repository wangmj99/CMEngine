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
        public TDClient TDClient { get; set; }

        [JsonIgnore]
        private EReaderMonitorSignal signal;

        [JsonIgnore]
        private object trade_locker = new object();

        [JsonIgnore]
        public bool IsInitialized { get; set; }

        [JsonIgnore]
        public static TradeManager Instance = new TradeManager();

        [JsonIgnore]
        public  Broker Broker { get; set; }

        private TradeManager() { }

        public void Init(Broker broker)
        {
            Broker = broker;
            if (broker == Broker.IB)
            {
                if (IBClient != null)
                {
                    Disconnect();
                    Thread.Sleep(500);
                }

                signal = new EReaderMonitorSignal();
                IBClient = new IBClient(signal);

                IBClient.OpenOrder += HandleOpenOrderMsg;
                IBClient.ExecutionDetails += HandleExecutionMsg;
                IBClient.OrderStatus += HandleOrderStatusMsg;
            }else if (broker == Broker.TD)
            {
                TDClient = TDClient.Instance;
                TDClient.ExecutionDetails += HandleExecutionMsg;
                TDClient.OrderStatus += HandleOrderStatusMsg;
            }

            IsInitialized = true;
        }

        public bool IsConnected { get { return IBClient != null && IBClient.IsConnected; } }

        public bool Connect (string ip, int port, int clientID)
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
                throw ex;
            }

            return true;
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
                    throw ex;
                }
            }
            else
            {
                Log.Info("IB is already disconnected");
            }
        }

        private void HandleOrderStatusMsg(OrderStatusMessage msg)
        {
            if(msg.Status!= Constant.OrderSubmitted)
                Log.Info(string.Format("Receive Order Status Msg. OrderID: {0}, Status: {1}", msg.OrderId, msg.Status));

            bool updated = false;
            try
            {
                var parent = ParentOrderManager.Instance.FindAssociatedParentOrderByChildID(msg.OrderId);
                if (parent != null)
                {
                    updated = parent.HandleOrderStatusMsg(msg);
                }else
                {
                    Log.Error(string.Format("Cannot find associated Parent Order."));
                }

                if (IsInitialized && updated)
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
                var parent = ParentOrderManager.Instance.FindAssociatedParentOrderByChildID(msg.Execution.OrderId);
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
            //Log.Info("Receive Open Order Msg. " + string.Format("Open OrderID {0}, ClientID {1} ", msg.Order.OrderId, msg.Order.ClientId));
        }

        public TradeOrder PlaceOrder(int parentOrderID, TradeType tradeType, string symbol, double price, double qty, string exchange = null, OrderType orderType = OrderType.LMT)
        {
            if (tradeType != TradeType.Buy && tradeType != TradeType.Sell) throw new Exception("Unsupported TradeType: " + tradeType);

            TradeOrder res = null;
                lock (trade_locker)
            {
                int orderID = -1;
                if (Broker == Broker.IB)
                {
                    orderID = IBClient.PlaceOrder(symbol, price, qty, tradeType, exchange, orderType);
                }else
                {
                    orderID = TDClient.PlaceOrder(symbol, price, qty, tradeType, exchange, orderType);
                }
                res = new TradeOrder();
                res.ParentOrderID = parentOrderID;
                res.OrderID = orderID;
                res.Status = TradeOrderStatus.PendingSubmit;
                res.Side = tradeType.ToString();
                res.Price = price;

                ParentOrderManager.Instance.AddChildOrder(res);

                if (IsInitialized)
                    StateManager.Save();
            }

            Log.Info(string.Format("Place order ID {0}, TradeType {1}, symbol {2}, price {3}, qty {4}, exchange {5}, parentOrderID {6}", 
                res.OrderID, tradeType, symbol, price, qty, exchange, parentOrderID));

            return res;
        }

        public TradeOrder PlaceTrailStopOrder(int parentOrderID, TradeType tradeType, string symbol, double qty, double trailStopPrice, double trailPct, string exchange = null)
        {
            TradeOrder res = null;
            int orderID = -1;
            lock (trade_locker)
            {

                if (Broker == Broker.IB)
                {
                    orderID = IBClient.PlaceTrailStopOrder(symbol, qty, trailStopPrice, trailPct, tradeType, exchange);
                }
                else
                {
                    //orderID = TDClient.PlaceOrder(symbol, price, qty, tradeType, exchange, orderType);
                }

                res = new TradeOrder();
                res.ParentOrderID = parentOrderID;
                res.OrderID = orderID;
                res.Status = TradeOrderStatus.PendingSubmit;
                res.Side = tradeType.ToString();
                res.TrailStopPrice = trailStopPrice;
                res.TrailingPct = trailPct;

                if (IsInitialized)
                    StateManager.Save();
            }

            Log.Info(string.Format("Place trailingstop order ID {0}, TradeType {1}, symbol {2}, qty {3}, trailStoppPrice {4}, trailingPct {5}%, exchange {6}",
                orderID, tradeType, symbol, qty, trailStopPrice, trailPct*100, exchange));

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
                        if (Broker == Broker.IB)
                        {
                            IBClient.ClientSocket.cancelOrder(order.OrderID);
                        }else if(Broker == Broker.TD)
                        {
                            TDClient.CancelOrder(order.OrderID);
                        }
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

        public void RequestGlobalCancel()
        {
            lock (trade_locker)
            {
                try
                {
                    Log.Info("Request Global cancel!");
                    if (Broker == Broker.IB)
                    {
                        IBClient.ClientSocket.reqGlobalCancel();
                    }else if(Broker == Broker.TD)
                    {
                        TDClient.RequestGlobalCancel();
                    }
                }catch(Exception ex)
                {
                    Log.Error("Failed to request global cancel. Error: " + ex.Message);
                    Log.Error(ex.StackTrace);
                }
            }
        }

        public TDOrder GetTDOrderById(int orderId)
        {
            return TDClient.GetOrderByID(orderId);

        }

        public List<TradeExecution> GetTDTradeExecution(TDOrder order)
        {
            List<TradeExecution> res = new List<TradeExecution>();

            if (order.orderActivityCollection != null)
            {
                foreach (var item in order.orderActivityCollection)
                {
                    if (item.executionLegs != null)
                    {
                        foreach (var leg in item.executionLegs)
                        {
                            TradeExecution exe = new TradeExecution(leg, order);
                            res.Add(exe);
                        }
                    }
                }
            }
            return res;
        }


    }
}
