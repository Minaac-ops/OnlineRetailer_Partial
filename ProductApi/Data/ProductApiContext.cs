using Microsoft.EntityFrameworkCore;
using ProductApi.Models;
using Shared;

namespace ProductApi.Data
{
    public class ProductApiContext : DbContext
    {
        public ProductApiContext(DbContextOptions<ProductApiContext> options)
            : base(options)
        {
        }

        public DbSet<Product> Products { get; set; }
    }
}
