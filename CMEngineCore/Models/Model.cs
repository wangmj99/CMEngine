using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CMEngineCore.Models
{
    public enum TradeType
    {
        Buy,
        Sell,
        BuyToCover,
        SellShort,
        Hold
    }

    public class IBExchanges
    {
        public static string NYSE = "NYSE";
        public static string NASDAQ = "ISLAND";
    }
}
