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

        public Dictionary<int, TradeMapEntry> InitializeTradeMap(RollingAlgo algo)
        {
            Dictionary<int, TradeMapEntry> res = new Dictionary<int, TradeMapEntry>();

            double buyPrice = algo.BeginPrice;
            double sellPrice = int.MaxValue;
            
            double qty = (algo.IsShare) ? algo.ShareOrDollarAmt : Math.Floor(algo.ShareOrDollarAmt / algo.BeginPrice);
            for (int i = 0; i<=algo.ScaleLevel; i++)
            {
                TradeMapEntry entry = new TradeMapEntry();
                entry.Level = i;
                
                if (i == 0)
                {
                    entry.TargetBuyPrice = buyPrice;           
                    entry.TargetSellPrice = sellPrice;
                    entry.TargetQty = qty;
                }
                else
                {

                    double tmpprice = (algo.IsPctScaleFactor) ? buyPrice * (1 - algo.ScaleFactor) : buyPrice - algo.ScaleFactor;
                    buyPrice = tmpprice <= 0 ? 0 : Util.NormalizePrice(tmpprice);
                    entry.TargetBuyPrice = buyPrice;

                    double tmpprice2 = (algo.IsPctScaleFactor) ? buyPrice / (1 - algo.ScaleFactor) : buyPrice + algo.ScaleFactor;
                    sellPrice = tmpprice2 <= 0 ? 0 : Util.NormalizePrice(tmpprice2);
                    entry.TargetSellPrice = sellPrice;

                    qty = (algo.IsShare) ? algo.ShareOrDollarAmt : Math.Floor(algo.ShareOrDollarAmt / entry.TargetBuyPrice);
                    entry.TargetQty = qty;

                    double adj = 0d;
                    if (algo.IsAdjPct) adj = entry.Level * entry.TargetQty * algo.AdjQty / (double)100;
                    else adj = algo.AdjQty * entry.Level;
                    entry.TargetQty += adj;
                    if (entry.TargetQty < 0) entry.TargetQty = 0;




                }
                res[i] = entry;
            }

            return res;
        }
    }
}
