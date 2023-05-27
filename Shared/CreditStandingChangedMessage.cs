using System.Collections.Generic;

namespace Shared
{
    public class CreditStandingChangedMessage
    {
        public Dictionary<string, object> Header { get; set; } = new();
        public int CustomerId { get; set; }
    }
}