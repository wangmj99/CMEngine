using IBApi;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CMEngineCore.Models
{
    abstract class OrderMessage
    {
        public int OrderId { get; set; }
    }

    class OpenOrderMessage : OrderMessage
    {
        public Contract Contract { get; set; }
        public Order Order { get; set; }
        public OrderState OrderStae { get; set; }

        public OpenOrderMessage(int orderId, Contract contract, Order order, OrderState orderState)
        {
            OrderId = orderId;
            Contract = contract;
            Order = order;
            OrderStae = orderState;
        }
    }
}
