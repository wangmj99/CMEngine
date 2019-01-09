using CMEngineCore.Models;
using log4net;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace CMEngineCore
{
    public abstract class Algo
    {
        public abstract void Eval(ParentOrder parentOrder);
        public abstract void HandleExecutionMsg(ParentOrder parentOrder, TradeExecution tradeExecution);
    }

    public class RollingAlgo : Algo
    {
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


            TradeOrder order = parentOrder.GetChildOrderByID(execution.Execution.OrderId);
            if(order == null)
            {
                Log.Error(string.Format("Cannot find trade order, orderID: {0}", execution.Execution.OrderId));

            }else
            {
                int exeLevel = int.Parse(order.Notes);
                TradeMap[exeLevel].CurrentQty += execution.Execution.Shares;
                if (TradeMap[exeLevel].CurrentQty > TradeMap[exeLevel].TargetQty)
                {
                    //should not hit here
                    Log.Error(string.Format("Overbot Qty detected. level: {0}, qty: {1}, target qty: {2}", 
                        exeLevel, TradeMap[exeLevel].CurrentQty, TradeMap[exeLevel].TargetQty));
                }

                UpdateTradeMapCurrentLevel(execution);

                if (TradeMap[CurrentLevel].Filled)
                {
                    TradeMap[CurrentLevel].WasFilledSellOnPartial = true;
                    GenerateTradeMapNextLevel(execution);
                }
            }
        }

        private void GenerateTradeMapNextLevel(TradeExecution execution)
        {
            throw new NotImplementedException();
        }

        private void UpdateTradeMapCurrentLevel(TradeExecution execution)
        {
            throw new NotImplementedException();
        }

        private void HandleSellExecution(ParentOrder parentOrder, TradeExecution execution)
        {
            TradeOrder order = parentOrder.GetChildOrderByID(execution.Execution.OrderId);
            if (order == null)
            {
                Log.Error(string.Format("Cannot find trade order, orderID: {0}", execution.Execution.OrderId));

            }
            else
            {
                int exeLevel = int.Parse(order.Notes);
                TradeMap[exeLevel].CurrentQty -= execution.Execution.Shares;

                if(TradeMap[exeLevel].CurrentQty < 0)
                {
                    //should not hit here
                    Log.Error(string.Format("Negative CurrentQty detected. level: {0}, qty: {1}", exeLevel, TradeMap[exeLevel].CurrentQty));
                }

                if (TradeMap[exeLevel].CurrentQty <= 0)
                {
                    TradeMap.Remove(CurrentLevel);
                    CurrentLevel--;

                    for(int i = CurrentLevel+1; i <= ScaleLevel; i++)
                    {
                        if (TradeMap.ContainsKey(i)) TradeMap.Remove(i);
                    }

                    if (CurrentLevel >= 0)
                    {
                        GenerateTradeMapNextLevel(execution);
                    }
                }
            }
        }

        public override void HandleExecutionMsg(ParentOrder parentOrder, TradeExecution tradeExecution)
        {
            if(tradeExecution.Execution.Side.ToUpper() == Constant.Buy)
            {
                HandleBuyExecution(parentOrder, tradeExecution);

            }else if (tradeExecution.Execution.Side.ToUpper() == Constant.Sell)
            {
                HandleSellExecution(parentOrder, tradeExecution);
            }
            else
            {
                Log.Error(string.Format("Unsupported execution. Side {0}", tradeExecution.Execution.Side.ToUpper()));
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