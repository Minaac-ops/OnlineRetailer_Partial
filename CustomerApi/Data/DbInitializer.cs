using System;
using System.Collections.Generic;
using System.Linq;
using CustomerApi.Models;
using Shared;

namespace CustomerApi.Data
{
    public class DbInitializer : IDbInitializer
    {
        public void Initialize(CustomerApiContext context)
        {
            context.Database.EnsureDeleted();
            context.Database.EnsureCreated();

            // Look for any Products
            if (context.Customers.Any())
            {
                return;   // DB has been seeded
            }

            List<Customer> orders = new List<Customer>
            {
                new Customer {CompanyName = "Normal",BillingAddress = "Torvet 4,6700 Esbjerg", Email = "normal@email.com", PhoneNo = 12345678, ShippingAddress = "Torvet 4, 6700 Esbjerg",CreditStanding = true}
            };

            context.Customers.AddRange(orders);
            context.SaveChanges();
        }
    }
}