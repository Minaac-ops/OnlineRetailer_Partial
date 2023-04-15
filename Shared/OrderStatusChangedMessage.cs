using System.Collections;
using System.Collections.Generic;

namespace Shared
{
    public class OrderStatusChangedMessage
    {
        public int OrderId { get; set; }
        public IList<OrderLine>? OrderLine { get; set; }
        public string? Topic { get; set; }
    }
}