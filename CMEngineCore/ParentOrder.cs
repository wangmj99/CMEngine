using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CMEngineCore
{
    public class ParentOrder
    {
        public string ID { get; set; }
        public string Symbol { get; set; }
        public double OpenQty { get; set; }
        public double Qty { get; set; }

        public ParentOrder(string ID, string symbol, double openQty, TradeMap tradeMap)
        {
            this.ID = ID;
            this.Symbol = symbol;
            this.OpenQty = openQty;
            this.TradeMap = tradeMap;
            Qty = openQty;
        }



        public TradeMap TradeMap { get; set; }

        public void Evaluate()
        {
            //on time elapse event, evalute and place order

        }

        public void ProcessExecution()
        {
            //Add execution

            //Update TradeMap
        }

        
    }
}
