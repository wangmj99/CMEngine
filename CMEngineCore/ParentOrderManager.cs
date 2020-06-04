using CMEngineCore.Models;
using log4net;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;

namespace CMEngineCore
{
    public class ParentOrderManager
    {
        public static string DataFile = "ParentOrderManager.dat";

        private static readonly ILog Log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        [JsonIgnore]
        public static ParentOrderManager Instance = new ParentOrderManager();

        [JsonIgnore]
        private object locker = new object();

        [JsonIgnore]
        private System.Timers.Timer m_timer;

        [JsonIgnore]
        private volatile bool timerInProcess = false;

        [JsonIgnore]
        public  bool IsInit = false;

        public List<ParentOrder> ParentOrderList { get; set; }

        //parentOrderID => List of Child Order IDs
        public Dictionary<int, List<int>> Parent_Child_Order_Map = new Dictionary<int, List<int>>();

        //Child Order ID => Parent Order ID
        public Dictionary<int, int> Child_Parent_Order_Map = new Dictionary<int, int>();

        [JsonIgnore]
        public bool IsStarted { get { return m_timer != null && m_timer.Enabled; } }

        private ParentOrderManager()
        {
            ParentOrderList = new List<ParentOrder>();
            m_timer = new System.Timers.Timer();
            m_timer.Elapsed += new System.Timers.ElapsedEventHandler(OnTimeElapsed);
            m_timer.Interval = 15 * 1000;
        }

        private void OnTimeElapsed(object sender, ElapsedEventArgs e)
        {
            //Log.Info("Start ParentOrderManager OnTimeElapsed");
            if (timerInProcess) return;

            if (!TradeManager.Instance.IsConnected)
            {
                Log.Info("IB is disconnected, skip eval orders");
                return;
            }

            if (!Util.IsTradingHour())
            {
                Log.Info("Not trading hour");
                //return;
            }


            lock (locker)
            {
                timerInProcess = true;
                foreach (var p in ParentOrderList)
                {
                    if (p.IsActive)
                    {
                        try
                        {
                            Log.Info(string.Format("Start to evaluate parentOrder, ID: {0}, symbol {1}", p.ID, p.Symbol));
                            p.Eval();
                        }
                        catch (Exception ex)
                        {
                            Log.Error(ex.Message);
                            Log.Error(ex.StackTrace);
                        }
                    }
                    else
                    {
                        Log.Info(string.Format("Skip evaluate inactive parentOrder , ID: {0}, symbol {1}", p.ID, p.Symbol));
                    }
                }
                timerInProcess = false;
            }

            //Log.Info("Stop ParentOrderManager OnTimeElapsed");
        }

        public ParentOrder CreateParentOrder(string symbol, double openQty, Algo algo)
        {
            int id = -1;
            do
            {
                id = new Random().Next(1000 * 1000);
            } while (Parent_Child_Order_Map.ContainsKey(id));

            ParentOrder parentOrder = new ParentOrder(id, symbol, openQty, algo);
            Log.Info(string.Format("ParentOrder {0} is created, symbol {1}", id, symbol));
            return parentOrder;
        }

        public List<ParentOrder> GetAllParentOrders() { return ParentOrderList; }

        public ParentOrder GetParentOrderByParentID (int ID)
        {
            ParentOrder res = null;

            lock (locker)
            {
                foreach (ParentOrder p in ParentOrderList)
                {
                    if (p.ID == ID)
                    {
                        res = p;
                        break;
                    }
                }
            }
            return res;
        }

        public ParentOrder FindAssociatedParentOrderByChildID(int childOrderID)
        {
            ParentOrder res = null;
            lock (locker)
            {
                if (Child_Parent_Order_Map.ContainsKey(childOrderID))
                {
                    int pID = Child_Parent_Order_Map[childOrderID];
                    res = GetParentOrderByParentID(pID);
                }
            }

            return res;
        }

        public void AddParentOrder(ParentOrder parentOrder)
        {
            lock (locker)
            {
                ParentOrderList.Add(parentOrder);

                if (!Parent_Child_Order_Map.ContainsKey(parentOrder.ID))
                    Parent_Child_Order_Map[parentOrder.ID] = new List<int>();
            }
        }

        public void SetAllOrdertoCancelStatus()
        {
            lock (locker)
            {
                foreach(var p in ParentOrderList)
                {
                    foreach(var o in p.TradeOrders)
                    {
                        if(o.Status != TradeOrderStatus.Filled && o.Status != TradeOrderStatus.Cancelled)
                        {
                            o.Status = TradeOrderStatus.Cancelled;
                        }
                    }
                }
            }
        }

        public bool RemoveParentOrderByID (int ID)
        {
            int idx = -1;

            ParentOrder parentOrder = GetParentOrderByParentID(ID);
            if (parentOrder == null) return false;

            lock (locker)
            {
                //Cancel open orders for this parentOrder
                var openOrders = parentOrder.GetOpenOrders();
                TradeManager.Instance.CancelOrders(openOrders);
            }

            Thread.Sleep(2000);

            lock (locker)
            {

                if (Parent_Child_Order_Map.ContainsKey(ID))
                    Parent_Child_Order_Map.Remove(ID);

                List<int> remove = new List<int>();
                foreach(int childID in Child_Parent_Order_Map.Keys)
                {
                    if (Child_Parent_Order_Map[childID] == parentOrder.ID)
                        remove.Add(childID);
                }

                foreach (int item in remove)
                    Child_Parent_Order_Map.Remove(item);


                for (int i = 0; i < ParentOrderList.Count; i++)
                {
                    if (ParentOrderList[i].ID == ID)
                    {
                        idx = i;
                        break;
                    }
                }
                if (idx != -1) ParentOrderList.RemoveAt(idx);
            }

            StateManager.Save();
            return idx != -1;
        }

        public void CloseParentOrderByID(int ID)
        {
            ParentOrder parent = GetParentOrderByParentID(ID);
            ParentOrderManager.Instance.StopParentOrder(ID);
            Thread.Sleep(500);
            //Please mkt order
            Log.Info(string.Format("Close parent order {0},  qty: {1}, market order ", parent.ID, parent.Qty));
            var res =TradeManager.Instance.PlaceOrder(ID, TradeType.Sell, parent.Symbol, 0, parent.Qty, null, OrderType.MKT);
            
        }

        public void AddChildOrder(TradeOrder tradeOrder)
        {
            if (!Parent_Child_Order_Map.ContainsKey(tradeOrder.ParentOrderID))
            {
                return;
            }

            lock (locker)
            {
                ParentOrder pOrder = GetParentOrderByParentID(tradeOrder.ParentOrderID);
                pOrder.AddChildOrder(tradeOrder);
                Parent_Child_Order_Map[tradeOrder.ParentOrderID].Add(tradeOrder.OrderID);
                Child_Parent_Order_Map[tradeOrder.OrderID] = tradeOrder.ParentOrderID;
            }
        }

        public void Save(string filename)
        {
            Util.SerializeObject<ParentOrderManager>(Instance, filename);
        }

        public void StartParentOrder(int parentOrderID)
        {
            var p = GetParentOrderByParentID(parentOrderID);
            p.IsActive = true;

            StateManager.Save();
        }

        public void StopParentOrder(int parentOrderID)
        {
            var parentOrder = GetParentOrderByParentID(parentOrderID);
            parentOrder.IsActive = false;
            StateManager.Save();

            if (parentOrder == null) return ;

            if (TradeManager.Instance.IsConnected)
            {
                lock (locker)
                {
                    //Cancel open orders for this parentOrder
                    var openOrders = parentOrder.GetOpenOrders();
                    TradeManager.Instance.CancelOrders(openOrders);
                }
                Log.Info("IB is connected, cancelled child orders on parent order stop");
            }
            else
            {
                Log.Info("IB is disconnected, not cancelling child orders on parent order stop");
            }


        }

        public void StopAllParentOrders()
        {
            foreach(var p in ParentOrderList)
            {
                StopParentOrder(p.ID);
            }

            SetAllOrdertoCancelStatus();
        }

        public static ParentOrderManager PopulateStates(string filename)
        {
            ParentOrderManager res = ParentOrderManager.Instance;
            if (File.Exists(filename))
                res = Util.DeSerializeObject<ParentOrderManager>(filename);
            return res;
        }

        public void Start()
        {
            if (m_timer == null)
            {
                m_timer = new System.Timers.Timer();
                m_timer.Elapsed += new System.Timers.ElapsedEventHandler(OnTimeElapsed);
                m_timer.Interval = 30 * 1000;
            }
            m_timer.Start();
        }

        public void Stop()
        {
            if(m_timer!=null)
                m_timer.Stop();

        }

        public void Init()
        {
            StopAllParentOrders();
            Start();
            IsInit = true;
        }

    }
}
