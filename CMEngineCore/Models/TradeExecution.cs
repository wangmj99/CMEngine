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
    }
}
