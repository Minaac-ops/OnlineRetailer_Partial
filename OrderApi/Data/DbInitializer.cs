using System.Collections.Generic;
using System.Linq;
using System;
using OrderApi.Models;
using Shared;

namespace OrderApi.Data
{
    public class DbInitializer : IDbInitializer
    {
        // This method will create and seed the database.
        public void Initialize(OrderApiContext context)
        {
            context.Database.EnsureDeleted();
            context.Database.EnsureCreated();

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
                new Order { Date = DateTime.Today, OrderLines = orderLines,Status = OrderDto.OrderStatus.Completed}
            };

            context.Orders.AddRange(orders);
            context.OrderLines.AddRange(orderLines);
            context.SaveChanges();
        }
    }
}
