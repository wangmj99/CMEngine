using CMEngineCore.Models;
using log4net;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CMEngineCore
{
    public interface ITradeRule
    {
        List<TradeOrder> CreateOrder(ParentOrder parentOrder);
    }

    public class RollingAlgoBuyRule : ITradeRule
    {
        public List<TradeOrder> CreateOrder(ParentOrder parentOrder)
        {
            List<TradeOrder> res = new List<TradeOrder>();
            RollingAlgo algo = (RollingAlgo)parentOrder.Algo;

            TradeMapEntry entry = null;
            int orderLevel = -99;

            if (algo.CurrentLevel == -1)
            {
                //entry = algo.TradeMap[0];
                orderLevel = 0;
            }
            else
            {
                TradeMapEntry currEntry = algo.TradeMap[algo.CurrentLevel];
                if (currEntry.PartialFilled && !currEntry.WasFilledSellOnPartial)
                {
                    //On partial filled, if buy, then buy current level
                    orderLevel = algo.CurrentLevel;
                }
                else if (currEntry.Filled && algo.CurrentLevel < algo.ScaleLevel - 1)
                {
                    //buy next level
                    orderLevel = algo.CurrentLevel + 1;
                }
            }

            entry = algo.TradeMap.ContainsKey(orderLevel) ? algo.TradeMap[orderLevel] : null;

            if(entry != null)
            {
                double price = Util.AdjustOrderPrice(TradeType.Buy, parentOrder.Symbol, entry.TargetBuyPrice);
                double qty = entry.TargetQty - entry.CurrentQty;
                var order = TradeManager.Instance.PlaceOrder(parentOrder.ID, TradeType.Buy, parentOrder.Symbol, price, qty);
                order.Notes = orderLevel.ToString();
                res.Add(order);
            }

            return res;
        }
    }

    public class RollingAlgoSellRule : ITradeRule
    {
        public List<TradeOrder> CreateOrder(ParentOrder parentOrder)
        {
            List<TradeOrder> res = new List<TradeOrder>();

            RollingAlgo algo = (RollingAlgo)parentOrder.Algo;

            if (algo.CurrentLevel <= 0) return res;

            var entry = algo.TradeMap[algo.CurrentLevel];

            //if(entry.PartialFilled && !entry.SellOnPartil)
            //{
            //    //on partial filled, if buy, then sell upper level
            //    entry = algo.TradeMap[algo.CurrentLevel - 1];

            //}

            if (entry.Level > 0 && entry.WasFilledSellOnPartial)
            {
                double price = Util.AdjustOrderPrice(TradeType.Sell, parentOrder.Symbol, entry.TargetSellPrice);
                double qty = entry.CurrentQty;
                var order = TradeManager.Instance.PlaceOrder(parentOrder.ID, TradeType.Sell, parentOrder.Symbol, price, qty);
                order.Notes = algo.CurrentLevel.ToString();
                res.Add(order);
            }

            return res;
        }
    }

    public class RollingAlgoFirstLevelBuyRule : ITradeRule
    {
        public List<TradeOrder> CreateOrder(ParentOrder parentOrder)
        {
            List<TradeOrder> res = new List<TradeOrder>();
            RollingAlgo algo = (RollingAlgo)parentOrder.Algo;
            if (algo.BuyBackLvlZero)
            {
                TradeMapEntry entry = algo.TradeMap[algo.CurrentLevel];

                if(algo.CurrentLevel == 0 &&!entry.Filled && entry.WasFilledSellOnPartial)
                {
                    double price = Util.AdjustOrderPrice(TradeType.Buy, parentOrder.Symbol, entry.TargetBuyPrice);
                    double qty = entry.TargetQty - entry.CurrentQty;
                    var order = TradeManager.Instance.PlaceOrder(parentOrder.ID, TradeType.Buy, parentOrder.Symbol, price, qty);
                    order.Notes = algo.CurrentLevel.ToString();
                    res.Add(order);
                }
            }

            return res;
        }
    }

    public class RollingAlgoFirstLevelSellRule : ITradeRule
    {
        public double SellPct { get; set; }
        public double priceUpPct { get; set; }

        private static readonly ILog Log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);


        //sell certain pct of shares when price goes up, sell remaining shares if price went down below entry price
        public RollingAlgoFirstLevelSellRule(double sellPct, double priceUpPct)
        {
            this.SellPct = sellPct;
            this.priceUpPct = priceUpPct;
        }

        public List<TradeOrder> CreateOrder(ParentOrder parentOrder)
        {
            List<TradeOrder> res = new List<TradeOrder>();

            RollingAlgo algo = (RollingAlgo)parentOrder.Algo;

            if (algo.CurrentLevel != 0) return res;

            var entry = algo.TradeMap[algo.CurrentLevel];

            double lastPrice = MarketDataManager.Instance.GetLastPrice(parentOrder.Symbol);


            if (entry.WasFilledSellOnPartial )
            {
                double soldPct = 1 - entry.CurrentQty / entry.TargetQty;
                if (SellPct - soldPct > 0.001)
                {
                    double qty = SellPct * entry.TargetQty - (entry.TargetQty - entry.CurrentQty);
                    double price = Util.AdjustOrderPrice(TradeType.Sell, parentOrder.Symbol, entry.LastBuyPrice * (1 + priceUpPct));

                    if (Util.IsLimitPriceInMktRange(TradeType.Sell, parentOrder.Symbol, price, lastPrice))
                    {

                        var order = TradeManager.Instance.PlaceOrder(parentOrder.ID, TradeType.Sell, parentOrder.Symbol, price, qty);
                        order.Notes = algo.CurrentLevel.ToString();
                        res.Add(order);
                    }
                    else
                    {
                        Log.Info("Not place first level order due to outside last price range");
                        Log.Info(string.Format("Symbol:{0}, TradeType: {1}, Price: {2}, MarketPx: {3}",
                            parentOrder.Symbol, TradeType.Sell.ToString(), price, lastPrice
                            ));
                    }
                }
            }


            return res;
        }
    }
}
