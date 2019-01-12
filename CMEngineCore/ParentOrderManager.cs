using CMEngineCore.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CMEngineCore
{
    public class ParentOrderManager
    {
        public static string DataFile = "ParentOrderManager.dat";

        [JsonIgnore]
        public static ParentOrderManager Instance = new ParentOrderManager();

        [JsonIgnore]
        private object locker = new object();

        public List<ParentOrder> parentOrderList = new List<ParentOrder>();

        //parentOrderID => List of Child Order IDs
        public Dictionary<int, List<int>> Parent_Child_Order_Map = new Dictionary<int, List<int>>();

        //Child Order ID => Parent Order ID
        public Dictionary<int, int> Child_Parent_Order_Map = new Dictionary<int, int>();


        public ParentOrder CreateParentOrder(string symbol, double openQty, Algo tradeMap)
        {
            int id = -1;
            do
            {
                id = new Random().Next(1000 * 1000);
            } while (Parent_Child_Order_Map.ContainsKey(id));

            ParentOrder parentOrder = new ParentOrder(id, symbol, openQty, tradeMap);
            return parentOrder;
        }

        public List<ParentOrder> GetAllParentOrders() { return parentOrderList; }

        public ParentOrder GetParentOrderByParentID (int ID)
        {
            ParentOrder res = null;

            lock (locker)
            {
                foreach (ParentOrder p in parentOrderList)
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

        public ParentOrder GetParentOrderByChildID(int childOrderID)
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
                parentOrderList.Add(parentOrder);

                if (!Parent_Child_Order_Map.ContainsKey(parentOrder.ID))
                    Parent_Child_Order_Map[parentOrder.ID] = new List<int>();
            }
        }

        public bool RemoveParentOrderByID (int ID)
        {
            int idx = -1;

            lock (locker)
            {
                if (Parent_Child_Order_Map.ContainsKey(ID))
                    Parent_Child_Order_Map.Remove(ID);

                
                for (int i = 0; i < parentOrderList.Count; i++)
                {
                    if (parentOrderList[i].ID == ID)
                    {
                        idx = i;
                        break;
                    }
                }
                if (idx != -1) parentOrderList.RemoveAt(idx);
            }

            return idx != -1;
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

        }

        public void StopParentOrder(int parentOrderID)
        {

        }

    }
}
