using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ProductApi.Models;
using Shared;

namespace ProductApi.Data
{
    public class DbInitializer : IDbInitializer
    {
        // This method will create and seed the database.
        public async Task Initialize(ProductApiContext context)
        {
            await context.Database.EnsureCreatedAsync();
            await context.Database.EnsureCreatedAsync();

            // Look for any Products
            if (context.Products.Any())
            {
                return;   // DB has been seeded
            }

            List<Product> products = new List<Product>
            {
                new Product { Name = "Hammer", Price = 100, ItemsInStock = 10, ItemsReserved = 0 },
                new Product { Name = "Screwdriver", Price = 70, ItemsInStock = 20, ItemsReserved = 0 },
                new Product { Name = "Drill", Price = 500, ItemsInStock = 2, ItemsReserved = 0 }
            };

            await context.Products.AddRangeAsync(products);
            await context.SaveChangesAsync();
        }
    }
}
