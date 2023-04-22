using System.Collections.Generic;

namespace Shared
{
    public class OrderAcceptedMessage
    {
        public int OrderId { get; set; }
        public int? CustomerId { get; set; }
    }
}