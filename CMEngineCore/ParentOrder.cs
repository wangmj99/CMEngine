using CMEngineCore.Models;
using log4net;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace CMEngineCore
{
    public class ParentOrder
    {
        [JsonIgnore]
        private static readonly ILog Log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        public int ID { get; set; }
        public string Symbol { get; set; }
        public double InitialQty { get; set; }
        public double? AvailableCash { get; set; }
        public double Qty { get; set; }



        [JsonIgnore]
        public int NoOfTransactions
        {
            get
            {
                return this.Executions.Select(e => e.TradeType == Constant.ExecutionSell).Count();
            }
        }

        public bool IsActive { get; set; }

        public Algo Algo { get; set; }

        [JsonIgnore]
        private List<TradeOrder> lastSendOrders = new List<TradeOrder>();

        public double RealizedGain { get { return GetRealizedGain(); } }

        public double UnRealizedGain { get { return GetUnRealizedGain(); } }

        public double TotalGain { get { return this.RealizedGain + this.UnRealizedGain; } }

        [JsonIgnore]
        private object locker = new object();

        public List<TradeExecution> Executions = new List<TradeExecution>();

        public List<TradeOrder> TradeOrders { get; set; }

        public ParentOrder() { }

        public ParentOrder(int ID, string symbol, double openQty, Algo algo, double? cash = null)
        {
            this.ID = ID;
            this.Symbol = symbol;
            this.InitialQty = openQty;
            this.Algo = algo;
            this.AvailableCash = cash;
            Qty = openQty;

            this.IsActive = true;
            TradeOrders = new List<TradeOrder>();
        }

        public Dictionary<int, List<TradeExecution>> GetOrderExecutionDict()
        {
            Dictionary<int, List<TradeExecution>> res = new Dictionary<int, List<TradeExecution>>();

            lock (locker) { }
            foreach (var item in Executions)
            {
                int id = item.OrderID;
                if (!res.ContainsKey(id))
                {
                    res[id] = new List<TradeExecution>();                    
                }
                res[id].Add(item);
            }
        
            return res;
        }

        internal void HandleExecutionMsg(ExecutionMessage msg)
        {
            //Add execution
            TradeExecution tradeExec = new TradeExecution(msg.Execution);
            //tradeExec.ParentOrderID = this.ID;
            //tradeExec.Execution = msg.Execution;
            //tradeExec.Execution.LastLiquidity = null;
            //tradeExec.Symbol = this.Symbol;

            HandleTradeExecution(tradeExec);
        }

        private void HandleTradeExecution( TradeExecution tradeExec)
        {
            if(ContainExeution(tradeExec))
            {
                Log.Info(string.Format("Received duplicated Execution, ID: {0}, Side: {1}, Qty: {2}, Price: {3}", tradeExec.ExecID, tradeExec.Side, tradeExec.Shares, tradeExec.Price));
                return;
            }

            lock (locker)
            {
                Algo.HandleExecutionMsg(this, tradeExec);

                Executions.Add(tradeExec);

                if (tradeExec.Side == Constant.ExecutionBuy)
                {
                    Qty += tradeExec.Shares;

                }
                else if (tradeExec.Side == Constant.ExecutionSell)
                {
                    Qty -= tradeExec.Shares;

                }
            }
        }

        private bool ContainExeution(TradeExecution tradeExec)
        {
            bool res = false;

            foreach(var exe in Executions)
            {
                if(exe.ExecID.Trim() == tradeExec.ExecID.Trim())
                {
                    res = true;
                    break;
                }
            }

            return res;
        }

        internal TradeOrder GetChildOrderByID(int ID)
        {
            TradeOrder res = null;

            lock (locker)
            {
                foreach (var order in TradeOrders)
                {
                    if (order.OrderID == ID)
                    {
                        res = order;
                        break;
                    }
                }
            }
            return res;
        }

        internal void AddChildOrder(TradeOrder order)
        {
            if (order.ParentOrderID != this.ID) return;

            lock (locker)
            {
                TradeOrders.Add(order);
            }
        }

        internal bool HandleOrderStatusMsg(OrderStatusMessage msg)
        {
            bool res = false;
            TradeOrder order = GetChildOrderByID(msg.OrderId);

            if (order != null)
            {
                switch (msg.Status)
                {
                    case Constant.OrderCancelled:
                        if (order.Status != TradeOrderStatus.Cancelled)
                        {
                            order.Status = TradeOrderStatus.Cancelled;
                            res = true;
                        };
                        break;
                    case Constant.OrderApiCancelled:
                        if (order.Status != TradeOrderStatus.Cancelled)
                        {
                            order.Status = TradeOrderStatus.Cancelled;
                            res = true;
                        };
                        break; 
                    case Constant.OrderSubmitted:
                    case Constant.OrderPreSubmitted:
                        if (order.Status == TradeOrderStatus.PendingSubmit)
                        {
                            order.Status = TradeOrderStatus.Submitted;
                            res = true;
                        }
                        else
                        {
                            Log.Error("Received order submitted msg, however current order status is  " + msg.Status);
                        };
                        break;

                    case Constant.OrderFilled:
                        if (order.Status != TradeOrderStatus.Cancelled && order.Status!=TradeOrderStatus.PendingCxl)
                        {
                            order.Status = TradeOrderStatus.Filled;
                            res = true;
                        }
                        else
                        {
                            Log.Error("Received order OrderFilled msg, however current order status is  " + msg.Status);
                        }; 
                        break;
                }

            }else
            {
                Log.Error(string.Format("Error UpdateChildOrder. Cannot find order. ChildOrderID: {0}, ParentOrderID: {1}", msg.OrderId, this.ID));
            }

            return res;
        }

        internal bool UpdateTDOrderStatus(TDOrder tdOrder)
        {
            bool res = false;
            TradeOrder order = GetChildOrderByID(tdOrder.orderId);

            if (order != null)
            {
                switch (tdOrder.status)
                {
                    case TDConstantVal.OrderStatus_Canceled:
                    case TDConstantVal.OrderStatus_Rejected:
                        if (order.Status != TradeOrderStatus.Cancelled)
                        {
                            order.Status = TradeOrderStatus.Cancelled;
                            res = true;
                        };
                        break;
                    case TDConstantVal.OrderStatus_Filled:
                        if (order.Status != TradeOrderStatus.Cancelled && order.Status != TradeOrderStatus.PendingCxl)
                        {
                            order.Status = TradeOrderStatus.Filled;
                            res = true;
                        }
                        else
                        {
                            Log.Error("Received order OrderFilled msg, however current order status is  " + tdOrder.status);
                        };
                        break;
                    case TDConstantVal.OrderStatus_Accepted:
                    case TDConstantVal.OrderStatus_AwaitReview:
                    case TDConstantVal.OrderStatus_Queued:
                    case TDConstantVal.OrderStatus_Working:
                        if (order.Status == TradeOrderStatus.PendingSubmit || order.Status == TradeOrderStatus.Submitted)
                        {
                            order.Status = TradeOrderStatus.Submitted;
                            res = true;
                        }
                        else
                        {
                            Log.Error("Received order submitted msg, however current order status is  " + tdOrder.status);
                        };
                        break;


                }

            }
            else
            {
                Log.Error(string.Format("Error UpdateChildOrder. Cannot find order. ChildOrderID: {0}, ParentOrderID: {1}", tdOrder.orderId, this.ID));
            }

            return res;
        }

        internal void UpdateAvailableCash()
        {

        }


        internal List<TradeOrder> GetOpenOrders()
        {
            List<TradeOrder> res = new List<TradeOrder>();

            lock (locker)
            {
                foreach(var order in TradeOrders)
                {
                    if(order.Status == TradeOrderStatus.Submitted || order.Status == TradeOrderStatus.PendingSubmit)
                    {
                        res.Add(order);
                    }
                }
            }

            return res;
        }



        internal void Eval()
        {
            lock (locker)
            {
                /*
                 1. On waking up, check if last open orders has been filled on canclled.
                 2. if none of order filled or cancelled then sleep again
                 3. else cancel all op
                 
                 * */

                //if(lastSendOrders.Count==0 && this.Qty == this.InitialQty)
                if (lastSendOrders.Count == 0 && GetOpenOrders().Count == 0)
                {
                    try
                    {
                        List<TradeOrder> orders = Algo.Eval(this);
                        lastSendOrders.AddRange(orders);
                    }
                    catch (Exception ex)
                    {
                        Log.Error(ex.Message);
                        Log.Error(ex.StackTrace);
                    }
                    return;
                }


                bool changed = false;
                if (lastSendOrders.Count > 0)
                {
                    foreach (var order in lastSendOrders)
                    {
                        if(TradeManager.Instance.Broker == Broker.TD)
                        {
                            TDOrder tdOrder = TradeManager.Instance.GetTDOrderById(order.OrderID);
                            if (tdOrder != null)
                            {
                                //TODO: update tradeOrder status  -- refer handle order status msg
                                UpdateTDOrderStatus(tdOrder);

                                //TODO: check if there is new execution
                                List<TradeExecution> executions = TradeManager.Instance.GetTDTradeExecution(tdOrder);
                                List<TradeExecution> newExecutions = FindNewExecutions(executions, order);

                                if (newExecutions.Count > 0)
                                {
                                    foreach(var exec in newExecutions)
                                    {
                                        HandleTradeExecution(exec);
                                    }
                                }
                            }
                        }

                        if (order.Status == TradeOrderStatus.Filled || order.Status == TradeOrderStatus.Cancelled)
                        {
                            changed = true;
                            Log.Info("Order status changed, start cancel open orders!");
                            break;
                        }
                    }
                }

                if (changed)
                {
                    try
                    {
                        //cancel all open orders, synchronized call, wait for cancel event back
                        var openOrders = GetOpenOrders();
                        if (openOrders.Count > 0)
                        {
                            TradeManager.Instance.CancelOrders(openOrders);
                            //wait for cancel back
                            Thread.Sleep(1000);

                            if(TradeManager.Instance.Broker == Broker.TD)
                            {
                                //update cancel order status here.
                                foreach(var order in openOrders)
                                {
                                    TDOrder tdOrder = TradeManager.Instance.GetTDOrderById(order.OrderID);
                                    if (tdOrder != null)
                                    {
                                        UpdateTDOrderStatus(tdOrder);
                                    }
                                }
                            }
                        }
                        lastSendOrders.Clear();

                        //place buy and sell order
                        List<TradeOrder> orders = Algo.Eval(this);
                        lastSendOrders.AddRange(orders);
                    
                    }
                    catch (Exception ex)
                    {
                        Log.Error(ex.Message);
                        Log.Error(ex.StackTrace);
                    }
                }

            }
        }

        public List<TradeExecution> FindNewExecutions(List<TradeExecution> executions, TradeOrder order)
        {
            List<TradeExecution> res = new List<TradeExecution>();

            if (executions.Count > 0)
            {
                var dict = GetOrderExecutionDict();
                if (!dict.ContainsKey(order.OrderID) || dict[order.OrderID].Count == 0)
                {
                    res.AddRange(executions);
                }
                else if (dict[order.OrderID].Count < res.Count)
                {
                    int idx = dict[order.OrderID].Count;
                    
                    executions.Sort();

                    for (int i = idx; i < executions.Count; i++)
                    {
                        res.Add(executions[i]);
                    }
                }
            }
            return res;
        }

        public double GetRealizedGain()
        {
            double res = 0;
            List<TradeExecution> buyList = Executions.Where(e => e.TradeType == Constant.ExecutionBuy).ToList();
            buyList.Sort();

            var sellList = Executions.Where(e => e.TradeType == Constant.ExecutionBuy).ToList();
            sellList.Sort();


            double totalQty = 0;
            
            for(int i = sellList.Count-1; i >= 0; i--)
            {
                res += sellList[i].Shares * sellList[i].Price;
                totalQty += sellList[i].Shares;
            }

            //LIFO
            for(int i = buyList.Count-1; i >= 0 && totalQty > 0; i--)
            {
                var exe = buyList[i];
                double tempQty = Math.Min(totalQty, exe.Shares);
                res -= (exe.Price * tempQty);
                totalQty -= tempQty;               
            }

            return res;
        }

        public double GetUnRealizedGain()
        {

            double mktValue = this.Qty * MarketDataManager.Instance.GetLastPrice(this.Symbol);

            var buyList = Executions.Where(e => e.TradeType == Constant.ExecutionBuy).ToList();
            buyList.Sort();

            double qty = this.Qty;
            double costBasis = 0;

            for (int i = 0; i < buyList.Count && qty > 0; i++)
            {
                var exe = buyList[i];
                costBasis += exe.Price * Math.Min(qty, exe.Shares);
                qty -= Math.Min(qty, exe.Shares);
            }

            return mktValue - costBasis;
        }
        
    }
}
