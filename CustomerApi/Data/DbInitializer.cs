using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CustomerApi.Models;
using Shared;

namespace CustomerApi.Data
{
    public class DbInitializer : IDbInitializer
    {
        public async Task Initialize(CustomerApiContext context)
        {
            await context.Database.EnsureDeletedAsync();
            await context.Database.EnsureCreatedAsync();

            // Look for any Products
            if (context.Customers.Any())
            {
                return;   // DB has been seeded
            }

            List<Customer> orders = new List<Customer>
            {
                new Customer {CompanyName = "Normal",BillingAddress = "Torvet 4,6700 Esbjerg", Email = "normal@email.com", PhoneNo = 12345678, ShippingAddress = "Torvet 4, 6700 Esbjerg",CreditStanding = true}
            };

            await context.Customers.AddRangeAsync(orders);
            await context.SaveChangesAsync();
        }
    }
}