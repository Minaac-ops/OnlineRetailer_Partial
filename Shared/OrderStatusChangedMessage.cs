using System.Collections.Generic;

namespace Shared
{
    public class OrderStatusChangedMessage
    {
        //published by order api when an order is created.
        public int CustomerId { get; set; }
        public IList<OrderLine>? OrderLines { get; set; }
    }
}