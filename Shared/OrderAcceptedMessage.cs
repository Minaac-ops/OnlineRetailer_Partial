using System.Collections.Generic;

namespace Shared
{
    public class OrderAcceptedMessage
    {
        public Dictionary<string, object> Header { get; set; } = new();
        public int OrderId { get; set; }
        public int? CustomerId { get; set; }
    }
}