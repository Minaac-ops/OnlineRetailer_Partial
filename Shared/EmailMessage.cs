using System.Collections.Generic;

namespace Shared
{
    public class EmailMessage
    {
        public Dictionary<string, object> Header { get; set; } = new();
        public int CustomerId { get; set; }
        public int OrderId { get; set; }
    }
}