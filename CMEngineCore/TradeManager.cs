using log4net;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using IBApi;
using Newtonsoft.Json;
using CMEngineCore.Models;
using System.Threading;

namespace CMEngineCore
{
    public class TradeManager
    {
        public static string DataFile = "TradeManager.dat";
        private static readonly ILog Log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        [JsonIgnore]
        public IBClient IBClient { get; set; }

        [JsonIgnore]
        private EReaderMonitorSignal signal;

        [JsonIgnore]
        private object trade_locker = new object();

        [JsonIgnore]
        public bool IsInitialized { get; set; }

        [JsonIgnore]
        public static TradeManager Instance = new TradeManager();

        private TradeManager() { }

        public void Init()
        {
            if (IBClient != null)
            {
                Disconnect();
            }

            signal = new EReaderMonitorSignal();
            IBClient = new IBClient(signal);

            IBClient.OpenOrder += HandleOpenOrderMsg;
            IBClient.ExecutionDetails += HandleExecutionMsg;
            IBClient.OrderStatus += HandleOrderStatusMsg;

            IsInitialized = true;
        }

        public bool IsConnected { get { return IBClient != null && IBClient.IsConnected; } }

        public void Connect (string ip, int port, int clientID)
        {
            Log.Info("Connecting IB");
            try
            {
                IBClient.ClientSocket.eConnect(ip, port, clientID);

                var reader = new EReader(IBClient.ClientSocket, signal);
                reader.Start();

                new Thread(() => { while (IBClient.ClientSocket.IsConnected()) { signal.waitForSignal(); reader.processMsgs(); } }) { IsBackground = true }.Start();
            }
            catch(Exception ex)
            {
                Log.Error("Error on connect IB, message: " + ex.Message);
                Log.Error("Error on connect IB, StackTrace: " + ex.StackTrace);
            }
        }

        private void Disconnect()
        {
            if (IBClient.IsConnected)
            {
                try
                {
                    IBClient.ClientSocket.eDisconnect();
                    Log.Info("Disconnecting IB");
                }catch(Exception ex)
                {
                    Log.Error("Disconnect IB error. Message: " + ex.Message);
                    Log.Error(ex.StackTrace);
                }
            }
            else
            {
                Log.Info("IB is already disconnected");
            }
        }

        private void HandleOrderStatusMsg(OrderStatusMessage obj)
        {
            throw new NotImplementedException();
        }

        private void HandleExecutionMsg(ExecutionMessage obj)
        {
            throw new NotImplementedException();
        }

        private void HandleOpenOrderMsg(OpenOrderMessage obj)
        {
            throw new NotImplementedException();
        }


    }
}
