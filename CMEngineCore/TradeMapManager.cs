using log4net;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CMEngineCore
{
    public class TradeMapManager
    {
        private static readonly ILog Log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        public static TradeMapManager Instance = new TradeMapManager();
        private TradeMapManager() { }

        public Dictionary<int, TradeMapEntry> GetTradeMapByParentID(int parentOrderID)
        {
            Dictionary<int, TradeMapEntry> res = null;

            ParentOrder p = ParentOrderManager.Instance.GetParentOrderByParentID(parentOrderID);

            if (p != null)
            {
                res = ((RollingAlgo)p.Algo).TradeMap;
            }else
            {
                Log.Error(string.Format("Cannot find parent order ID: " + parentOrderID));
            }

            return res;
        }

        public bool UpdateTradeMapByParentID(int parentOrderID, Dictionary<int, TradeMapEntry> map, int currLvl)
        {
            bool res = false;
            ParentOrder p = ParentOrderManager.Instance.GetParentOrderByParentID(parentOrderID);
            if (p != null)
            {
                RollingAlgo algo = (RollingAlgo)p.Algo;
                algo.TradeMap = map;
                algo.CurrentLevel = currLvl;

                res = true;
            }
            else
            {
                Log.Error(string.Format("Cannot find parent order ID: " + parentOrderID));
            }
            return res;
        }

        public bool IsValid(int parentOrderID, Dictionary<int, TradeMapEntry> map, int currLvl)
        {
            bool res = false;
            ParentOrder p = ParentOrderManager.Instance.GetParentOrderByParentID(parentOrderID);
            if (p != null)
            {
                RollingAlgo algo = (RollingAlgo)p.Algo;
                algo.TradeMap = map;
                algo.CurrentLevel = currLvl;

                //check qty
                //check curr level

                res = true;
            }
            else
            {
                Log.Error(string.Format("Cannot find parent order ID: " + parentOrderID));
            }

            return res;
        }
    }
}
