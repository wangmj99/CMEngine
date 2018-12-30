using log4net;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using IBApi;
using Newtonsoft.Json;

namespace CMEngineCore
{
    public class TradeManager
    {
        public static string DataFile = "tradeManager.dat";
        private static readonly ILog Log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        [JsonIgnore]
        public IBClient IBClient { get; set; }


        public List<ParentOrder> ParentOrders = new List<ParentOrder>();

        public Dictionary<string, List<int>> Parent_Child_Order_Map = new Dictionary<string, List<int>>();

        public ParentOrder CreateParentOrder(string symbol, double openQty, TradeMap tradeMap)
        {
            string id = Guid.NewGuid().ToString();
            ParentOrder parentOrder = new ParentOrder(id, symbol, openQty, tradeMap);
            ParentOrders.Add(parentOrder);
            Parent_Child_Order_Map[id] = new List<int>();

            return parentOrder;
        }

        public bool RemoveParentOrder(ParentOrder parentOrder)
        {
            if (Parent_Child_Order_Map.ContainsKey(parentOrder.ID))
                Parent_Child_Order_Map.Remove(parentOrder.ID);

            int temp = -1;
            for( int idx =0; idx< ParentOrders.Count; idx++)
            {
                var p = ParentOrders[idx];
                if(p.ID == parentOrder.ID)
                {
                    temp = idx;
                    break;
                }
            }

            if(temp != -1)
            {
                ParentOrders.RemoveAt(temp);
                
                return true;
            }else
            {
                return false;
            }
        }



    }
}
