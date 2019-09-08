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
        public string TradeType { get; private set; }
        public double Shares { get; private set; }
        public double Price { get; private set; }
        //public string Symbol { get; set; }
        //public string ExecID { get { return Execution.ExecId; } }
        public int OrderID { get; private set; }
        public DateTime Time { get; private set; }
        public string Side { get; private set; }


        public TradeExecution(Execution ibExecution)
        {
            TradeType = ibExecution.Side;
            Shares = ibExecution.Shares;
            Price = ibExecution.Price;
            OrderID = ibExecution.OrderId;
            Time = DateTime.ParseExact(ibExecution.Time, "yyyyMMdd  HH:mm:ss", System.Globalization.CultureInfo.InvariantCulture);
            Side = ibExecution.Side.ToUpper();
        }

        public TradeExecution(TDExecutionLeg TDExecutionLeg)
        {
            TradeType = TDExecutionLeg.side;
            Shares = TDExecutionLeg.quantity;
            Price = TDExecutionLeg.price;
            OrderID = TDExecutionLeg.orderId;
            Time = TDExecutionLeg.time;
            Side = TDExecutionLeg.side.ToUpper();
        }

        public int CompareTo(TradeExecution other)
        {
            if (other == null) return 1;
            return this.Time.CompareTo(other.Time);
        }
    }
}
