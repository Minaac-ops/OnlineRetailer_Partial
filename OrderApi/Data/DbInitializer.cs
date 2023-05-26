using System.Collections.Generic;
using System.Linq;
using System;
using System.Threading.Tasks;
using OrderApi.Models;
using Shared;

namespace OrderApi.Data
{
    public class DbInitializer : IDbInitializer
    {
        // This method will create and seed the database.
        public async Task Initialize(OrderApiContext context)
        {
            await context.Database.EnsureDeletedAsync();
            await context.Database.EnsureCreatedAsync();

            // Look for any Products
            if (context.Orders.Any())
            {
                return;   // DB has been seeded
            }

            List<OrderLine> orderLines = new List<OrderLine>()
            {
                new OrderLine() {ProductId = 1, Quantity = 2,},
                new OrderLine() {ProductId = 4, Quantity = 1,}
            };

            List<Order> orders = new List<Order>
            {
                new Order { Date = DateTime.Today, OrderLines = orderLines,Status = OrderDto.OrderStatus.Completed, CustomerId = 1}
            };

            await context.Orders.AddRangeAsync(orders);
            await context.OrderLines.AddRangeAsync(orderLines);
            await context.SaveChangesAsync();
        }
    }
}
