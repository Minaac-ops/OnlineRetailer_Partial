using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System;
using System.Threading.Tasks;
using Monitoring;
using OrderApi.Models;
using Shared;

namespace OrderApi.Data
{
    public class OrderRepository : IRepository<Order>
    {
        private readonly OrderApiContext db;

        public OrderRepository(OrderApiContext context)
        {
            db = context;
        }

        async Task<Order> IRepository<Order>.Add(Order entity)
        {
            using var activity = MonitorService.ActivitySource.StartActivity();
            if (entity == null)
            {
                throw new Exception("An empty order can't be saved to the database.");
            }
            entity.Date ??= DateTime.Now;
            
            var newOrder = await db.Orders.AddAsync(entity);
            Console.WriteLine("OrderRepo: After adding to db "+ entity);
            await db.SaveChangesAsync();;
            return new Order
            {
                CustomerId = newOrder.Entity.CustomerId,
                Date = newOrder.Entity.Date,
                OrderLines = newOrder.Entity.OrderLines,
                Id = newOrder.Entity.Id,
                Status = newOrder.Entity.Status
            };
        }

        async Task IRepository<Order>.Edit(Order entity)
        {
            if (entity == null)
            {
                throw new Exception("Couldn't find customer with id "+entity.Id+" to update.");
            }
            Console.WriteLine(entity.Status);
            
            db.Entry(entity).State = EntityState.Modified;
            await db.SaveChangesAsync();
        }

        async Task<Order> IRepository<Order>.Get(int id)
        {
            using var activity = MonitorService.ActivitySource.StartActivity();
            var entity = db.Orders
                .Where(o => o.Id == id)
                .Select(order => new Order()
                {
                    Id = order.Id,
                    Date = order.Date,
                    CustomerId = order.CustomerId,
                    Status = order.Status,
                    OrderLines = order.OrderLines.Select(ol => new OrderLine()
                    {
                        Id = ol.Id,
                        ProductId = ol.ProductId,
                        OrderId = ol.OrderId,
                        Quantity = ol.Quantity
                    }).ToList(),
                }).FirstOrDefaultAsync();
            return await entity;
        }

        async Task<IEnumerable<Order>> IRepository<Order>.GetAll()
        {
            using var activity = MonitorService.ActivitySource.StartActivity();
            MonitorService.Log.Here().Debug("Entered method GetAll");
            var select = db.Orders.Select(order => new Order()
            {
                Id = order.Id,
                Date = order.Date,
                CustomerId = order.CustomerId,
                Status = order.Status,
                OrderLines = order.OrderLines.Select(ol => new OrderLine()
                {
                    Id = ol.Id,
                    ProductId = ol.ProductId,
                    Quantity = ol.Quantity,
                    OrderId = ol.OrderId,
                }).ToList(),
            });
            return await @select.ToListAsync();
        }

        async Task IRepository<Order>.Remove(int id)
        {
            var order = await db.Orders.FirstOrDefaultAsync(p => p.Id == id);
            db.Orders.Remove(order);
            await db.SaveChangesAsync();
        }

        async Task<IEnumerable<Order>> IRepository<Order>.GetByCustomerId(int customerId)
        {
            var entities = db.Orders
                .Where(o => o.CustomerId == customerId)
                .Select(order => new Order()
                {
                    Id = order.Id,
                    Date = order.Date,
                    CustomerId = order.CustomerId,
                    Status = order.Status,
                    OrderLines = order.OrderLines.Select(ol => new OrderLine()
                    {
                        Id = ol.Id,
                        ProductId = ol.ProductId,
                        OrderId = ol.OrderId,
                        Quantity = ol.Quantity
                    }).ToList(),
                }).ToListAsync();
            return await entities;
        }
    }
}
