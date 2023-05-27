using System.Collections.Generic;

namespace Shared
{
    public class ProductDto
    {
        public Dictionary<string, object> Header { get; set; } = new();
        public int Id { get; set; }
        public string? Name { get; set; }
        public decimal Price { get; set; }
        public int ItemsInStock { get; set; }
        public int ItemsReserved { get; set; }
    }
}
