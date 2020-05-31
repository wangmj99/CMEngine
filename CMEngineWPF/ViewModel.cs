using CMEngineCore;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CMEngineWPF
{
    public class ParentOrderListViewModel: INotifyCollectionChanged
    {
        private List<ParentOrder> _parentOrders;

        public List<ParentOrder> ParentOrders
        {
            get { return _parentOrders; }
            set
            {
                _parentOrders = value;
                OnCollectionChanged("ParentOrderList");
            }
        }

        public event NotifyCollectionChangedEventHandler CollectionChanged;

        protected void OnCollectionChanged(string collectionName)
        {

        }
    }
}
