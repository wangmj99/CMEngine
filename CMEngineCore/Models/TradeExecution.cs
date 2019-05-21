using IBApi;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CMEngineCore.Models
{
    public class TradeExecution
    {
        public Execution Execution { get; set; }
        public int ParentOrderID { get; set; }
        public string TradeType { get { return Execution.Side; } }
        public double ExecutionPrice { get { return Execution.Price; } }
        public double Shares { get { return Execution.Shares; } }
        public string Symbol { get; set; }
        public string ExecID { get { return Execution.ExecId; } }
        public int OrderID { get { return Execution.OrderId; } }
        public DateTime Time { get
            {
                return DateTime.ParseExact(Execution.Time, "yyyyMMdd  HH:mm:ss", System.Globalization.CultureInfo.InvariantCulture);
            }
        }

        public int CompareTo(TradeExecution other)
        {
            if (other == null) return 1;
            return this.Time.CompareTo(other.Time);
        }
    }
}
