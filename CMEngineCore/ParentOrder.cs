using CMEngineCore.Models;
using log4net;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CMEngineCore
{
    public class ParentOrder
    {
        private static readonly ILog Log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        public int ID { get; set; }
        public string Symbol { get; set; }
        public double OpenQty { get; set; }
        public double? AvailableCash { get; set; }
        public double Qty { get; set; }

        private object locker = new object();

        public List<TradeExecution> Executions = new List<TradeExecution>();

        public List<TradeOrder> TradeOrders = new List<TradeOrder>();

        public ParentOrder(int ID, string symbol, double openQty, TradeMap tradeMap, double? cash = null )
        {
            this.ID = ID;
            this.Symbol = symbol;
            this.OpenQty = openQty;
            this.TradeMap = tradeMap;
            this.AvailableCash = cash;
            Qty = openQty;
        }

        public TradeMap TradeMap { get; set; }

        public void Evaluate()
        {
            //on time elapse event, evalute and place order

        }

        public void HandleExecutionMsg(ExecutionMessage msg)
        {
            //Add execution
            TradeExecution tradeExec = new TradeExecution();
            tradeExec.ParentOrderID = this.ID;
            tradeExec.Execution = msg.Execution;

            lock (locker)
            {
                Executions.Add(tradeExec);

                if (msg.Execution.Side.ToUpper() == Constant.Buy)
                {
                    Qty += msg.Execution.Shares;

                }
                else if (msg.Execution.Side.ToUpper() == Constant.Sell)
                {
                    Qty -= msg.Execution.Shares;

                }               
            }

            //Update TradeMap
        }

        public TradeOrder GetChildOrderByID(int ID)
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

        public void AddChildOrder(TradeOrder order)
        {
            if (order.ParentOrderID != this.ID) return;

            lock (locker)
            {
                TradeOrders.Add(order);
            }
        }

        public void HandleOrderStatusMsg(OrderStatusMessage msg)
        {
            TradeOrder order = GetChildOrderByID(msg.OrderId);

            if (order != null)
            {
                switch (msg.Status)
                {
                    case Constant.OrderCancelled: order.Status = TradeOrderStatus.Cancelled;break;
                    case Constant.OrderApiCancelled: order.Status = TradeOrderStatus.Cancelled;break;
                    case Constant.OrderSubmitted: order.Status = TradeOrderStatus.Submitted;break;
                    case Constant.OrderFilled: order.Status = TradeOrderStatus.Filled;break;
                }

            }else
            {
                Log.Error(string.Format("Error UpdateChildOrder. Cannot find order. ChildOrderID: {0}, ParentOrderID: {1}", msg.OrderId, this.ID));
            }
        }

        public void UpdateAvailableCash()
        {

        }
        
    }
}
