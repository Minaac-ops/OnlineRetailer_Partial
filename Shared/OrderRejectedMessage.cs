using System.Collections.Generic;

namespace Shared
{
    public class OrderRejectedMessage
    {
        public Dictionary<string, object> Header { get; set; } = new();
        public int OrderId { get; set; }
    }
}