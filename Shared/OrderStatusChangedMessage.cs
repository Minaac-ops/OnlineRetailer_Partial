using System.Collections;
using System.Collections.Generic;

namespace Shared
{
    public class OrderStatusChangedMessage
    {
        public Dictionary<string, object> Header { get; set; } = new();
        public int OrderId { get; set; }
        public IList<OrderLine>? OrderLine { get; set; }
    }
}