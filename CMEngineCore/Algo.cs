using CMEngineCore.Models;
using log4net;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace CMEngineCore
{
    public abstract class Algo
    {
        public abstract List<TradeOrder> Eval(ParentOrder parentOrder);
        public abstract void HandleExecutionMsg(ParentOrder parentOrder, TradeExecution tradeExecution);
    }

    public class RollingAlgo : Algo
    {
        [JsonIgnore]
        private static readonly ILog Log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        public string Name = "RollingAlgo";
        public int CurrentLevel { get; set; }
        public Dictionary<int, TradeMapEntry> TradeMap { get; set; }

        public double BeginPrice { get; set; }
        public double ScaleLevel { get; set; }
        public double ScaleFactor { get; set; } //percent or abs price difference
        public bool IsPctScaleFactor { get; set; }
        public double ShareOrDollarAmt { get; set; }
        public bool IsShare { get; set; }

        public double AdjQty { get; set; }
        public bool IsAdjPct { get; set; }

        public bool BuyBackLvlZero { get; set; }

        public List<ITradeRule> TradeRules { get; set; }

        public RollingAlgo(double beginPrice, double scaleLevel, double scaleFactor, bool isPct, double shareAmt, bool isShare, bool buyBackLvlZero, double adjQty, bool isAdjPct)
        {
            if (beginPrice <= 0) throw new Exception("Begin price cannot equal or less than zero!");

            this.BeginPrice = beginPrice;
            this.ScaleLevel = scaleLevel;
            this.ScaleFactor = scaleFactor;
            this.IsPctScaleFactor = isPct;
            this.ShareOrDollarAmt = shareAmt;
            this.IsShare = isShare;
            this.BuyBackLvlZero = buyBackLvlZero;
            this.AdjQty = adjQty;
            this.IsAdjPct = isAdjPct;

            this.CurrentLevel = -1;

            InitTradeMap();

        }
                      
        private void InitTradeMap()
        {
            TradeMap = new Dictionary<int, TradeMapEntry>();
            //CurrentLevel = -1;
            TradeMapEntry entry = new TradeMapEntry();
            entry.Level = 0;
            entry.TargetBuyPrice = BeginPrice;
            entry.TargetQty = (IsShare) ? ShareOrDollarAmt : Math.Floor(ShareOrDollarAmt / entry.TargetBuyPrice);
            entry.TargetSellPrice = int.MaxValue;
            TradeMap[entry.Level] = entry;
        }

        public override List<TradeOrder> Eval(ParentOrder parentOrder)
        {
            List<TradeOrder> res = new List<TradeOrder>();
            foreach (var rule in TradeRules)
            {
                var list = rule.CreateOrder(parentOrder);

                if (list != null && list.Count > 0)
                    res.AddRange(list);
            }
            
            return res;
        }

        private void HandleBuyExecution(ParentOrder parentOrder, TradeExecution execution)
        {
            /*
             * check if buy execution create new tradeMap entry
             * 1.curr level =-1;
             * 2. Curre level filled and next level exist
             * 3. curr level partial filled but wasfilled and next level exist
             * 4. curr level is zero and partial filled and buyback enabled
             * 
             * */


            TradeOrder order = parentOrder.GetChildOrderByID(execution.OrderID);
            if (order == null)
            {
                Log.Error(string.Format("Cannot find trade order, orderID: {0}", execution.OrderID));

            }
            else
            {
                int exeLevel = int.Parse(order.Notes);
                TradeMap[exeLevel].CurrentQty += execution.Shares;
                if (TradeMap[exeLevel].CurrentQty > TradeMap[exeLevel].TargetQty)
                {
                    //should not hit here
                    Log.Error(string.Format("Overbot Qty detected. level: {0}, qty: {1}, target qty: {2}",
                        exeLevel, TradeMap[exeLevel].CurrentQty, TradeMap[exeLevel].TargetQty));
                }


                TradeMap[exeLevel].LastBuyPrice = execution.Price;
                TradeMap[exeLevel].TargetSellPrice = (exeLevel == 0) ? int.MaxValue : TradeMap[exeLevel - 1].LastBuyPrice;

                CurrentLevel = Math.Max(CurrentLevel, exeLevel);

                if (TradeMap[CurrentLevel].Filled)
                {
                    TradeMap[CurrentLevel].WasFilledSellOnPartial = true;
                    GenerateTradeMapNextLevel(TradeMap[CurrentLevel].LastBuyPrice);
                }

                Log.Info("After bot execution." + Util.PrintTradeMapCurrLvl(this));
            }
        }

        private void GenerateTradeMapNextLevel(double currBuyPrice)
        {
            if (CurrentLevel < this.ScaleLevel - 1)
            {
                TradeMapEntry entry = new TradeMapEntry();
                entry.Level = CurrentLevel + 1;

                double price = (IsPctScaleFactor) ? currBuyPrice * (1 - ScaleFactor) :
                    currBuyPrice - ScaleFactor;


                entry.TargetBuyPrice = price <= 0 ? 0 : Util.NormalizePrice(price);

                if (this.IsShare)
                    entry.TargetQty = this.ShareOrDollarAmt;
                else
                    entry.TargetQty = Math.Floor(this.ShareOrDollarAmt / entry.TargetBuyPrice);

                double adj = 0d;
                if (this.IsAdjPct) adj = entry.Level * entry.TargetQty * this.AdjQty / (double)100;
                else adj = this.AdjQty * entry.Level;

                entry.TargetQty += adj;
                if (entry.TargetQty < 0) entry.TargetQty = 0;

                TradeMap[entry.Level] = entry;

            }
        }

        private void UpdateTradeMapCurrentLevel(TradeExecution execution)
        {
            //TradeMapEntry entry = TradeMap[execution]
            //entry.LastBuyPrice = execution.Execution.Price;
            //entry.TargetSellPrice = (CurrentLevel == 0) ? int.MaxValue : TradeMap[CurrentLevel - 1].LastBuyPrice;
        }

        private void HandleSellExecution(ParentOrder parentOrder, TradeExecution execution)
        {
            TradeOrder order = parentOrder.GetChildOrderByID(execution.OrderID);
            if (order == null)
            {
                Log.Error(string.Format("Cannot find trade order, orderID: {0}", execution.OrderID));

            }
            else
            {
                if (string.IsNullOrWhiteSpace(order.Notes))
                {
                    Log.Warn("HandleSellExecution error. Cannot resolve execution level due to emtpy order notes");
                }
                else
                {

                    int exeLevel = int.Parse(order.Notes);
                    TradeMap[exeLevel].CurrentQty -= execution.Shares;

                    if (TradeMap[exeLevel].CurrentQty < 0)
                    {
                        //should not hit here
                        Log.Error(string.Format("Negative CurrentQty detected. level: {0}, qty: {1}", exeLevel, TradeMap[exeLevel].CurrentQty));
                    }

                    if (TradeMap[exeLevel].CurrentQty <= 0)
                    {
                        TradeMap.Remove(CurrentLevel);
                        CurrentLevel--;

                        for (int i = CurrentLevel + 1; i <= ScaleLevel; i++)
                        {
                            if (TradeMap.ContainsKey(i)) TradeMap.Remove(i);
                        }

                        if (CurrentLevel >= 0)
                        {
                            GenerateTradeMapNextLevel(TradeMap[CurrentLevel].LastBuyPrice);
                        }
                    }
                }
                Log.Info("After sld execution." + Util.PrintTradeMapCurrLvl(this));
            }
        }

        public override void HandleExecutionMsg(ParentOrder parentOrder, TradeExecution tradeExecution)
        {
            if(tradeExecution.Side.ToUpper() == Constant.ExecutionBuy)
            {
                HandleBuyExecution(parentOrder, tradeExecution);

            }else if (tradeExecution.Side.ToUpper() == Constant.ExecutionSell)
            {
                HandleSellExecution(parentOrder, tradeExecution);
            }
            else
            {
                Log.Error(string.Format("Unsupported execution. Side {0}", tradeExecution.Side));
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