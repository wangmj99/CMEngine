using CMEngineCore.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CMEngineCore
{
    public interface ITradeRule
    {
        List<int> CreateOrder(ParentOrder parentOrder);
    }

    public class RollingAlgoBuyRule : ITradeRule
    {
        public List<int> CreateOrder(ParentOrder parentOrder)
        {
            List<int> res = new List<int>();
            RollingAlgo algo = (RollingAlgo)parentOrder.Algo;

            TradeMapEntry entry = null;

            if (algo.CurrentLevel == -1)
            {
                entry = algo.TradeMap[0];
            }
            else
            {
                TradeMapEntry currEntry = algo.TradeMap[algo.CurrentLevel];
                if (currEntry.PartialFilled && !currEntry.WasFilledSellOnPartial)
                {
                    //On partial filled, if bugy, then fill current level
                    entry = currEntry;
                }
                else if (algo.CurrentLevel < algo.ScaleLevel - 1)
                {
                    //buy next level
                    entry = algo.TradeMap[algo.CurrentLevel + 1];
                }
            }

            if(entry != null)
            {
                double price = Util.AdjustOrderPrice(TradeType.Buy, parentOrder.Symbol, entry.TargetBuyPrice);
                double qty = entry.TargetQty - entry.CurrentQty;
                int orderID = TradeManager.Instance.PlaceOrder(parentOrder.ID, TradeType.Buy, parentOrder.Symbol, price, qty);
                res.Add(orderID);
            }

            return res;
        }
    }

    public class RollingAlgoSellRule : ITradeRule
    {
        public List<int> CreateOrder(ParentOrder parentOrder)
        {
            List<int> res = new List<int>();

            RollingAlgo algo = (RollingAlgo)parentOrder.Algo;

            if (algo.CurrentLevel <= 0) return res;

            var entry = algo.TradeMap[algo.CurrentLevel];

            //if(entry.PartialFilled && !entry.SellOnPartil)
            //{
            //    //on partial filled, if buy, then sell upper level
            //    entry = algo.TradeMap[algo.CurrentLevel - 1];

            //}

            if (entry.Level > 0)
            {
                double price = Util.AdjustOrderPrice(TradeType.Sell, parentOrder.Symbol, entry.TargetSellPrice);
                double qty = entry.CurrentQty;
                int orderID = TradeManager.Instance.PlaceOrder(parentOrder.ID, TradeType.Sell, parentOrder.Symbol, price, qty);
                res.Add(orderID);
            }

            return res;
        }
    }

    public class RollingAlgoFirstLevelBuyRule : ITradeRule
    {
        public List<int> CreateOrder(ParentOrder parentOrder)
        {
            List<int> res = new List<int>();
            RollingAlgo algo = (RollingAlgo)parentOrder.Algo;
            if (algo.BuyBackLvlZero)
            {
                TradeMapEntry entry = algo.TradeMap[algo.CurrentLevel];

                if(algo.CurrentLevel == 0 &&!entry.Filled && entry.WasFilledSellOnPartial)
                {
                    double price = Util.AdjustOrderPrice(TradeType.Buy, parentOrder.Symbol, entry.TargetBuyPrice);
                    double qty = entry.TargetQty - entry.CurrentQty;
                    int orderID = TradeManager.Instance.PlaceOrder(parentOrder.ID, TradeType.Buy, parentOrder.Symbol, price, qty);
                    res.Add(orderID);
                }
            }

            return res;
        }
    }

    public class RollingAlgoFirstLevelSellRule : ITradeRule
    {
        public double SellPct { get; set; }
        public double priceUpPct { get; set; }

        //sell certain pct of shares when price goes up, sell remaining shares if price went down below entry price
        public RollingAlgoFirstLevelSellRule(double sellPct, double priceUpPct)
        {
            this.SellPct = sellPct;
            this.priceUpPct = priceUpPct;
        }

        public List<int> CreateOrder(ParentOrder parentOrder)
        {
            List<int> res = new List<int>();

            RollingAlgo algo = (RollingAlgo)parentOrder.Algo;

            if (algo.CurrentLevel != 0) return res;

            var entry = algo.TradeMap[algo.CurrentLevel];

            double lastPrice = MarketDataManager.Instance.GetLastPrice(parentOrder.Symbol);

            if (entry.WasFilledSellOnPartial && lastPrice <= entry.LastBuyPrice * 1.01)
            {
                double price = Util.AdjustOrderPrice(TradeType.Sell, parentOrder.Symbol, entry.LastBuyPrice);
                double qty = entry.CurrentQty;
                int orderID = TradeManager.Instance.PlaceOrder(parentOrder.ID, TradeType.Sell, parentOrder.Symbol, price, qty);
                res.Add(orderID);
            } 
            else
            {
                double soldPct = 1 - entry.CurrentQty / entry.TargetQty;
                if(SellPct-soldPct>0.001)
                {
                    double qty = SellPct * entry.TargetQty - (entry.TargetQty -entry.CurrentQty);
                    double price = Util.AdjustOrderPrice(TradeType.Sell, parentOrder.Symbol, entry.LastBuyPrice * (1 + priceUpPct));
                    int orderID = TradeManager.Instance.PlaceOrder(parentOrder.ID, TradeType.Sell, parentOrder.Symbol, price, qty);
                    res.Add(orderID);
                }
            }


            return res;
        }
    }
}
