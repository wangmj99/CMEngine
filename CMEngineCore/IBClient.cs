﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using IBApi;
using log4net;
using System.IO;
using System.Net.Sockets;
using CMEngineCore.Models;
using System.Configuration;

namespace CMEngineCore
{
    public class IBClient : EWrapper
    {
        public EClientSocket ClientSocket { get; set; }
        public int NextOrderId { get; set; }
        public SynchronizationContext sc { get; set; }

        private static readonly ILog Log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        string tradeAccount = string.IsNullOrWhiteSpace(ConfigurationManager.AppSettings["TradeAccount"]) ? string.Empty : ConfigurationManager.AppSettings["TradeAccount"].Trim();

        public IBClient(EReaderSignal signal)
        {
            ClientSocket = new EClientSocket(this, signal);
            sc = SynchronizationContext.Current;
        }


        public bool IsConnected { get; set; }

        public void accountDownloadEnd(string account)
        {
            throw new NotImplementedException();
        }

        public void accountSummary(int reqId, string account, string tag, string value, string currency)
        {
            throw new NotImplementedException();
        }

        public void accountSummaryEnd(int reqId)
        {
            throw new NotImplementedException();
        }

        public void accountUpdateMulti(int requestId, string account, string modelCode, string key, string value, string currency)
        {
            throw new NotImplementedException();
        }

        public void accountUpdateMultiEnd(int requestId)
        {
            throw new NotImplementedException();
        }

        public void bondContractDetails(int reqId, ContractDetails contract)
        {
            throw new NotImplementedException();
        }

        public void commissionReport(CommissionReport commissionReport)
        {
            //throw new NotImplementedException();
        }

        public void connectAck()
        {
            if (ClientSocket.AsyncEConnect)
                ClientSocket.startApi();
        }

        public void connectionClosed()
        {
            Log.Info("IB connection is closed!");
            IsConnected = false;
        }

        public void contractDetails(int reqId, ContractDetails contractDetails)
        {
            throw new NotImplementedException();
        }

        public void contractDetailsEnd(int reqId)
        {
            throw new NotImplementedException();
        }

        public void currentTime(long time)
        {
            DateTime start = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            DateTime date = start.AddMilliseconds(time).ToLocalTime();
        }

        public void deltaNeutralValidation(int reqId, DeltaNeutralContract deltaNeutralContract)
        {
            throw new NotImplementedException();
        }

        public void displayGroupList(int reqId, string groups)
        {
            throw new NotImplementedException();
        }

        public void displayGroupUpdated(int reqId, string contractInfo)
        {
            throw new NotImplementedException();
        }

        private static List<string> disconnStrs = new List<string>() { "UNABLE", "READ", "END", "STREAM" };
        public void error(string str)
        {
            Log.Error(str);
            HashSet<string> strs = new HashSet<string>(str.ToUpper().Split(' ', '.'));

            bool res = true;
            foreach (string s in disconnStrs)
            {
                if(!strs.Contains(s))
                {
                    res = false;
                    continue;
                }
            }

            if (res)
                IsConnected = false;
        }

        public void error(Exception e)
        {
            Log.Error(e.Message);
            Log.Error(e.StackTrace);
            if(e is IOException || e is SocketException)
            {
                Log.Error("Disconnect IB on IO Exception/SocketException");
                IsConnected = false;
                throw e;
            }
        }

        public void error(int id, int errorCode, string errorMsg)
        {
            string str = string.Format("Error ID: {0}, Code: {1}, Msg: {2}", id, errorCode, errorMsg);
            Log.Error(str);
        }


        public event Action<ExecutionMessage> ExecutionDetails;
        public void execDetails(int reqId, Contract contract, Execution execution)
        {
            var tmp = ExecutionDetails;
            if (tmp != null)
                tmp(new ExecutionMessage(reqId, contract, execution));
        }

        public void execDetailsEnd(int reqId)
        {
            //TODO
        }

        public void familyCodes(FamilyCode[] familyCodes)
        {
            throw new NotImplementedException();
        }

        public void fundamentalData(int reqId, string data)
        {
            throw new NotImplementedException();
        }

        public void headTimestamp(int reqId, string headTimestamp)
        {
            throw new NotImplementedException();
        }

        public void histogramData(int reqId, HistogramEntry[] data)
        {
            throw new NotImplementedException();
        }

        public void historicalData(int reqId, Bar bar)
        {
            throw new NotImplementedException();
        }

        public void historicalDataEnd(int reqId, string start, string end)
        {
            throw new NotImplementedException();
        }

        public void historicalDataUpdate(int reqId, Bar bar)
        {
            throw new NotImplementedException();
        }

        public void historicalNews(int requestId, string time, string providerCode, string articleId, string headline)
        {
            throw new NotImplementedException();
        }

        public void historicalNewsEnd(int requestId, bool hasMore)
        {
            throw new NotImplementedException();
        }

        public void historicalTicks(int reqId, HistoricalTick[] ticks, bool done)
        {
            throw new NotImplementedException();
        }

        public void historicalTicksBidAsk(int reqId, HistoricalTickBidAsk[] ticks, bool done)
        {
            throw new NotImplementedException();
        }

        public void historicalTicksLast(int reqId, HistoricalTickLast[] ticks, bool done)
        {
            throw new NotImplementedException();
        }

        public void managedAccounts(string accountsList)
        {
            string list = string.Join(",", accountsList);
        }

        public void marketDataType(int reqId, int marketDataType)
        {
            throw new NotImplementedException();
        }

        public void marketRule(int marketRuleId, PriceIncrement[] priceIncrements)
        {
            throw new NotImplementedException();
        }

        public void mktDepthExchanges(DepthMktDataDescription[] depthMktDataDescriptions)
        {
            throw new NotImplementedException();
        }

        public void newsArticle(int requestId, int articleType, string articleText)
        {
            throw new NotImplementedException();
        }

        public void newsProviders(NewsProvider[] newsProviders)
        {
            throw new NotImplementedException();
        }

        public void nextValidId(int orderId)
        {
            string msg = "Next Order Id: " + orderId;
            NextOrderId = orderId;
            IsConnected = true;
            Log.Info("IB is connected!");
        }

        public event Action<OpenOrderMessage> OpenOrder;


        public void openOrder(int orderId, Contract contract, Order order, OrderState orderState)
        {
            if (OpenOrder != null)
                OpenOrder(new OpenOrderMessage(orderId, contract, order, orderState));
        }

        public void openOrderEnd()
        {
            //throw new NotImplementedException();
        }

        public void orderBound(long orderId, int apiClientId, int apiOrderId)
        {
            throw new NotImplementedException();
        }

        public event Action<OrderStatusMessage> OrderStatus;

        public void orderStatus(int orderId, string status, double filled, double remaining, double avgFillPrice, int permId, int parentId, double lastFillPrice, int clientId, string whyHeld, double mktCapPrice)
        {
            if(OrderStatus!=null)
                OrderStatus(new OrderStatusMessage(orderId, status, filled, remaining, avgFillPrice, permId, parentId, lastFillPrice, clientId, whyHeld, mktCapPrice));
        }

        public void pnl(int reqId, double dailyPnL, double unrealizedPnL, double realizedPnL)
        {
            throw new NotImplementedException();
        }

        public void pnlSingle(int reqId, int pos, double dailyPnL, double unrealizedPnL, double realizedPnL, double value)
        {
            throw new NotImplementedException();
        }

        public void position(string account, Contract contract, double pos, double avgCost)
        {
            throw new NotImplementedException();
        }

        public void positionEnd()
        {
            throw new NotImplementedException();
        }

        public void positionMulti(int requestId, string account, string modelCode, Contract contract, double pos, double avgCost)
        {
            throw new NotImplementedException();
        }

        public void positionMultiEnd(int requestId)
        {
            throw new NotImplementedException();
        }

        public void realtimeBar(int reqId, long time, double open, double high, double low, double close, long volume, double WAP, int count)
        {
            throw new NotImplementedException();
        }

        public void receiveFA(int faDataType, string faXmlData)
        {
            throw new NotImplementedException();
        }

        public void rerouteMktDataReq(int reqId, int conId, string exchange)
        {
            throw new NotImplementedException();
        }

        public void rerouteMktDepthReq(int reqId, int conId, string exchange)
        {
            throw new NotImplementedException();
        }

        public void scannerData(int reqId, int rank, ContractDetails contractDetails, string distance, string benchmark, string projection, string legsStr)
        {
            throw new NotImplementedException();
        }

        public void scannerDataEnd(int reqId)
        {
            throw new NotImplementedException();
        }

        public void scannerParameters(string xml)
        {
            throw new NotImplementedException();
        }

        public void securityDefinitionOptionParameter(int reqId, string exchange, int underlyingConId, string tradingClass, string multiplier, HashSet<string> expirations, HashSet<double> strikes)
        {
            throw new NotImplementedException();
        }

        public void securityDefinitionOptionParameterEnd(int reqId)
        {
            throw new NotImplementedException();
        }

        public void smartComponents(int reqId, Dictionary<int, KeyValuePair<string, char>> theMap)
        {
            throw new NotImplementedException();
        }

        public void softDollarTiers(int reqId, SoftDollarTier[] tiers)
        {
            throw new NotImplementedException();
        }

        public void symbolSamples(int reqId, ContractDescription[] contractDescriptions)
        {
            throw new NotImplementedException();
        }

        public void tickByTickAllLast(int reqId, int tickType, long time, double price, int size, TickAttribLast tickAttriblast, string exchange, string specialConditions)
        {
            throw new NotImplementedException();
        }

        public void tickByTickBidAsk(int reqId, long time, double bidPrice, double askPrice, int bidSize, int askSize, TickAttribBidAsk tickAttribBidAsk)
        {
            throw new NotImplementedException();
        }

        public void tickByTickMidPoint(int reqId, long time, double midPoint)
        {
            throw new NotImplementedException();
        }

        public void tickEFP(int tickerId, int tickType, double basisPoints, string formattedBasisPoints, double impliedFuture, int holdDays, string futureLastTradeDate, double dividendImpact, double dividendsToLastTradeDate)
        {
            throw new NotImplementedException();
        }

        public void tickGeneric(int tickerId, int field, double value)
        {
            throw new NotImplementedException();
        }

        public void tickNews(int tickerId, long timeStamp, string providerCode, string articleId, string headline, string extraData)
        {
            throw new NotImplementedException();
        }

        public void tickOptionComputation(int tickerId, int field, double impliedVolatility, double delta, double optPrice, double pvDividend, double gamma, double vega, double theta, double undPrice)
        {
            throw new NotImplementedException();
        }

        public void tickPrice(int tickerId, int field, double price, TickAttrib attribs)
        {
            throw new NotImplementedException();
        }

        public void tickReqParams(int tickerId, double minTick, string bboExchange, int snapshotPermissions)
        {
            throw new NotImplementedException();
        }

        public void tickSize(int tickerId, int field, int size)
        {
            throw new NotImplementedException();
        }

        public void tickSnapshotEnd(int tickerId)
        {
            throw new NotImplementedException();
        }

        public void tickString(int tickerId, int field, string value)
        {
            throw new NotImplementedException();
        }

        public void updateAccountTime(string timestamp)
        {
            throw new NotImplementedException();
        }

        public void updateAccountValue(string key, string value, string currency, string accountName)
        {
            throw new NotImplementedException();
        }

        public void updateMktDepth(int tickerId, int position, int operation, int side, double price, int size)
        {
            throw new NotImplementedException();
        }

        public void updateMktDepthL2(int tickerId, int position, string marketMaker, int operation, int side, double price, int size, bool isSmartDepth)
        {
            throw new NotImplementedException();
        }

        public void updateNewsBulletin(int msgId, int msgType, string message, string origExchange)
        {
            throw new NotImplementedException();
        }

        public void updatePortfolio(Contract contract, double position, double marketPrice, double marketValue, double averageCost, double unrealizedPNL, double realizedPNL, string accountName)
        {
            throw new NotImplementedException();
        }

        public void verifyAndAuthCompleted(bool isSuccessful, string errorText)
        {
            throw new NotImplementedException();
        }

        public void verifyAndAuthMessageAPI(string apiData, string xyzChallenge)
        {
            throw new NotImplementedException();
        }

        public void verifyCompleted(bool isSuccessful, string errorText)
        {
            throw new NotImplementedException();
        }

        public void verifyMessageAPI(string apiData)
        {
            throw new NotImplementedException();
        }

        public int PlaceOrder(string symbol, double price, double qty, TradeType tradeType, string exchange = null, OrderType orderType = OrderType.LMT)
        {
            if (tradeType != TradeType.Buy && tradeType != TradeType.Sell)
                throw new Exception(string.Format("Not support TradeType: {0}", tradeType));

            Contract contract = CreateContract(symbol, exchange);

            Order order = CreateOrder(qty, tradeType);
            order.OrderType = orderType.ToString();
            if (orderType == OrderType.LMT)
                order.LmtPrice = price;

            this.ClientSocket.placeOrder(NextOrderId, contract, order);
            int res = NextOrderId;
            NextOrderId++;

            return res;

        }

        public int PlaceTrailStopOrder(string symbol, double qty, double trailStopPrice, double trailingPct, TradeType tradeType, string exchange = null) 
        {
            Contract contract = CreateContract(symbol, exchange);
            Order order = CreateOrder(qty, tradeType);
            order.TrailStopPrice = trailStopPrice;
            order.TrailingPercent = trailingPct;
            order.OrderType = OrderType.TRAIL.ToString();

            this.ClientSocket.placeOrder(NextOrderId, contract, order);
            int res = NextOrderId;
            NextOrderId++;

            return res;
        }


        private Order CreateOrder(double qty, TradeType tradeType)
        {
            Order order = new Order();
            order.OrderId = NextOrderId;
            order.Action = (tradeType == TradeType.Buy) ? "BUY" : "SELL";
            order.TotalQuantity = qty;
            order.Account = "";
            order.ModelCode = "";
            order.Tif = "DAY";
            if (!string.IsNullOrEmpty(tradeAccount))
                order.Account = tradeAccount;
            return order;
        }

        private Contract CreateContract(string symbol, string exchange)
        {
            Contract contract = new Contract();
            contract.Symbol = symbol;
            contract.SecType = "STK";
            contract.Currency = "USD";
            contract.Exchange = "SMART";
            contract.PrimaryExch = exchange;
            if (contract.PrimaryExch == null)
                contract.PrimaryExch = symbol.Trim().Length < 4 ? IBExchanges.NASDAQ : IBExchanges.NYSE;

            return contract;
        }

        public void CancelOrder(int orderId)
        {
            ClientSocket.cancelOrder(orderId);
        }
    }
}
