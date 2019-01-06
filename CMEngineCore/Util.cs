﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using System.IO;
using CMEngineCore.Models;
using IBApi;
using log4net;

namespace CMEngineCore
{
    public static class Util
    {
        private static readonly ILog Log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        public static void SerializeObject<T>(T serializableObject, string fileName)
        {
            var indented = Newtonsoft.Json.Formatting.Indented;
            var setting = new JsonSerializerSettings()
            {
                TypeNameHandling = TypeNameHandling.All
            };

            string s = JsonConvert.SerializeObject(serializableObject, indented, setting);
            File.WriteAllText(fileName, s);
        }

        public static T DeSerializeObject<T>(string fileName)
        {
            using (StreamReader file = File.OpenText(fileName))
            {
                var settings = new JsonSerializerSettings()
                {
                    TypeNameHandling = TypeNameHandling.All
                };
                JsonSerializer serializer = JsonSerializer.Create(settings);
                return (T)serializer.Deserialize(file, typeof(T));
            }
        }


        public static string PrintExecutionMsg(ExecutionMessage msg)
        {
            Execution exe = msg.Execution;
            return string.Format("executioID {0}, orderID {1}, side {2}, price {3}, qty {4}, reqeustID {5}", exe.ExecId, exe.OrderId, exe.Side, exe.Price, exe.Shares, msg.ReqId);
        }

        public static double NormalizePrice (double price)
        {
            return Math.Round(price, 2, MidpointRounding.AwayFromZero);
        }

        public static bool IsLimitPriceInMktRange(TradeType tradeType, string symbol, double price)
        {
            bool res = false;
            double lastPrice = MarketDataManager.Instance.GetLastPrice(symbol);


            return res;
        }

        public static double AdjustOrderPrice(TradeType tradeType, string symbol, double price)
        {
            double res = -1;

            double lastPrice = MarketDataManager.Instance.GetLastPrice(symbol);
            if(lastPrice > 0)
            {
                if(tradeType == TradeType.Buy)
                {
                    //if buy limit way above current price, adjust to current price *1.01
                    if((price - lastPrice)/lastPrice >0.05d)
                    {
                        res = lastPrice * 1.01;
                    }
                }
                else if (tradeType == TradeType.Sell)
                {
                    if((lastPrice - price)/lastPrice > 0.05d)
                    {
                        res = lastPrice * 0.99;
                    }
                }
            }
            else
            {
                Log.Warn("Not able to get Quote for symbol " + symbol.ToUpper());
            }


            return NormalizePrice(res);
        }
    }
}