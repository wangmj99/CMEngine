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

        private List<TradeOrder> lastSendOrders = new List<TradeOrder>();

        [JsonIgnore]
        private object locker = new object();

        public List<TradeExecution> Executions = new List<TradeExecution>();

        public List<TradeOrder> TradeOrders { get; set; }

        public ParentOrder() { }

        public ParentOrder(int ID, string symbol, double openQty, Algo algo, double? cash = null )
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


        internal void HandleExecutionMsg(ExecutionMessage msg)
        {
            //Add execution
            TradeExecution tradeExec = new TradeExecution();
            tradeExec.ParentOrderID = this.ID;
            tradeExec.Execution = msg.Execution;
            tradeExec.Execution.LastLiquidity = null;
            tradeExec.Symbol = this.Symbol;

            lock (locker)
            {
                Algo.HandleExecutionMsg(this, tradeExec);

                Executions.Add(tradeExec);

                if (msg.Execution.Side.ToUpper() == Constant.ExecutionBuy)
                {
                    
                    Qty += msg.Execution.Shares;

                }
                else if (msg.Execution.Side.ToUpper() == Constant.ExecutionSell)
                {
                    Qty -= msg.Execution.Shares;

                }               
            }

            //Update TradeMap
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
                        if (order.Status != TradeOrderStatus.Submitted)
                        {
                            order.Status = TradeOrderStatus.Submitted;
                            res = true;
                        };
                        break;

                    case Constant.OrderFilled:
                        if (order.Status != TradeOrderStatus.Filled)
                        {
                            order.Status = TradeOrderStatus.Filled;
                            res = true;
                        };
                        break;
                }

            }else
            {
                Log.Error(string.Format("Error UpdateChildOrder. Cannot find order. ChildOrderID: {0}, ParentOrderID: {1}", msg.OrderId, this.ID));
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
                    catch(Exception ex)
                    {
                        Log.Error(ex.Message);
                        Log.Error(ex.StackTrace);
                    }
                }


                bool changed = false;
                if (lastSendOrders.Count > 0)
                {
                    foreach (var order in lastSendOrders)
                    {
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
                            Thread.Sleep(2000);
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

        public double GetRealizedGain()
        {
            double res = 0;
            var buyList = Executions.Where(e => e.TradeType == Constant.ExecutionBuy).ToList();
            buyList.Sort();

            var sellList = Executions.Where(e => e.TradeType == Constant.ExecutionBuy).ToList();
            sellList.Sort();

            double totalAmt = 0;
            double totalQty = 0;
            for(int i = sellList.Count-1; i >= 0; i--)
            {
                totalAmt += sellList[i].Execution.Shares * sellList[i].Execution.Price;
                totalQty += sellList[i].Execution.Shares;
            }



            return res;
        }
        
    }
}
