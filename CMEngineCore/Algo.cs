using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace CMEngineCore
{
    public abstract class Algo
    {
        public abstract void Eval(ParentOrder parentOrder);
    }

    public class RollingAlgo : Algo
    {
        public string Name = "RollingAlgo";
        public int CurrentLevel { get; set; }
        public Dictionary<int, TradeMapEntry> TradeMap { get; set; }

        public double BeginPrice { get; set; }
        public double ScaleLevel { get; set; }
        public double ScaleFactor { get; set; } //percent or abs price difference
        public bool IsPctScaleFactor { get; set; }
        public double ShareOrDollarAmt { get; set; }
        public bool IsShare { get; set; }

        public bool BuyBackLvlZero { get; set; }

        public List<ITradeRule> TradeRules { get; set; }


        public RollingAlgo(double beginPrice, double scaleLevel, double scaleFactor, bool isPct, double shareAmt, bool isShare, bool buyBackLvlZero)
        {
            if (beginPrice <= 0) throw new Exception("Begin price cannot equal or less than zero!");

            this.BeginPrice = beginPrice;
            this.ScaleLevel = scaleLevel;
            this.ScaleFactor = scaleFactor;
            this.IsPctScaleFactor = isPct;
            this.ShareOrDollarAmt = shareAmt;
            this.IsShare = isShare;
            this.BuyBackLvlZero = buyBackLvlZero;

            InitTradeMap();

        }
                      
        private void InitTradeMap()
        {
            TradeMap = new Dictionary<int, TradeMapEntry>();
            CurrentLevel = 0;
            TradeMapEntry entry = new TradeMapEntry();
            entry.Level = 0;
            entry.TargetBuyPrice = BeginPrice;
            entry.TargetQty = (IsShare) ? ShareOrDollarAmt : Math.Floor(ShareOrDollarAmt / entry.TargetBuyPrice);
            entry.TargetSellPrice = int.MaxValue;
            TradeMap[CurrentLevel] = entry;
        }

        public override void Eval(ParentOrder parentOrder)
        {
            foreach(var rule in TradeRules)
            {
                var list = rule.CreateOrder(parentOrder);
            }
        }
    }



    public class TradeMapEntry
    {
        public int Level { get; set; }
        public double TargetBuyPrice { get; set; }
        public double TargetSellPrice { get; set; }
        public double TargetQty { get; set; }

        public double CurrentQty { get; set; }
        public double LastBuyPrice { get; set; }

        [JsonIgnore]
        public bool Filled { get { return CurrentQty >= TargetQty; } }

        [JsonIgnore]
        public bool PartialFilled { get { return CurrentQty < TargetQty && CurrentQty > 0; } }

        public bool WasFilledSellOnPartial { get; set; }

    }
}