using IBApi;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CMEngineCore.Models
{
    public class TradeExecution: IComparable<TradeExecution>
    {
        //public Execution Execution { get; set; }
        //public int ParentOrderID { get; set; }
        public string TradeType { get; set; }
        public double Shares { get; set; }
        public double Price { get; set; }
        //public string Symbol { get; set; }
        public string ExecID { get; set; }
        public int OrderID { get; set; }
        public DateTime Time { get; set; }
        public string Side { get; set; }

        public TradeExecution() { }


        public TradeExecution(Execution ibExecution)
        {
            TradeType = ibExecution.Side;
            Shares = ibExecution.Shares;
            Price = ibExecution.Price;
            OrderID = ibExecution.OrderId;
            Time = DateTime.ParseExact(ibExecution.Time, "yyyyMMdd  HH:mm:ss", System.Globalization.CultureInfo.InvariantCulture);
            Side = ibExecution.Side.ToUpper();
            ExecID = ibExecution.ExecId;
        }

        public TradeExecution(TDExecutionLeg exe, TDOrder order)
        {
            TradeType = GetExecutionSide(order);
            Shares = exe.quantity;
            Price = exe.price;
            OrderID = order.orderId;
            Time = exe.time;
            Side = GetExecutionSide(order);
        }

        public int CompareTo(TradeExecution other)
        {
            if (other == null) return 1;
            return this.Time.CompareTo(other.Time);
        }

        private string GetExecutionSide(TDOrder order)
        {
            string side = order.orderLegCollection[0].instruction;
            if (side == TDConstantVal.OrderTradeType_Buy)
                return Constant.ExecutionBuy;
            else if (side == TDConstantVal.OrderTradeType_Sell)
                return Constant.ExecutionSell;
            else return "UNKNOWN";
        }
    }
}
