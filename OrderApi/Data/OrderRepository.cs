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
            MonitorService.Log.Here().Debug("OrderRepository Add");
            if (entity == null)
            {
                throw new Exception("An empty order can't be saved to the database.");
            }
            entity.Date ??= DateTime.Now;
            
            var newOrder = await db.Orders.AddAsync(entity);
          
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
            MonitorService.Log.Here().Debug("OrderRepository Edit");
            if (entity == null)
            {
                throw new Exception("Couldn't find customer with id "+entity.Id+" to update.");
            }
            
            db.Entry(entity).State = EntityState.Modified;
            await db.SaveChangesAsync();
        }

        async Task<Order> IRepository<Order>.Get(int id)
        {
            MonitorService.Log.Here().Debug("OrderRepository Get");
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
            MonitorService.Log.Here().Debug("OrderRepository GetAll");
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
            MonitorService.Log.Here().Debug("OrderRepository Remove");
            var order = await db.Orders.FirstOrDefaultAsync(p => p.Id == id);
            db.Orders.Remove(order);
            await db.SaveChangesAsync();
        }

        async Task<IEnumerable<Order>> IRepository<Order>.GetByCustomerId(int customerId)
        {
            MonitorService.Log.Here().Debug("OrderRepository GetByCustomerId");
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
