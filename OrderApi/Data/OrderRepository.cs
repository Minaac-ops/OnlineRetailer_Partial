using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System;
using System.Threading.Tasks;
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
            if (entity == null)
            {
                throw new Exception("An empty order can't be saved to the database.");
            }
            entity.Date ??= DateTime.Now;
            
            var newOrder = await db.Orders.AddAsync(entity);
            await db.SaveChangesAsync();
            return newOrder.Entity;
        }

        async Task IRepository<Order>.Edit(int id,Order entity)
        {
            var orderToUpdate = await db.Orders.FindAsync(id);

            if (orderToUpdate == null)
            {
                throw new Exception("Couldn't find customer with id " + id+" to update.");
            }
            orderToUpdate.Status = entity.Status;
            
            db.Entry(orderToUpdate).State = EntityState.Modified;
            await db.SaveChangesAsync();
        }

        async Task<Order> IRepository<Order>.Get(int id)
        {
            var entity = await db.Orders
                .Where(o => o.Id == id)
                .Select(order => new Order()
                {
                    Id = order.Id,
                    Date = order.Date,
                    OrderLines = order.OrderLines.Select(ol => new OrderLine()
                    {
                        Id = ol.Id,
                        ProductId = ol.ProductId,
                        Quantity = ol.Quantity
                    }).ToList(),
                }).FirstOrDefaultAsync();
            return entity;
        }

        async Task<IEnumerable<Order>> IRepository<Order>.GetAll()
        {
            var select = db.Orders.Select(order => new Order()
            {
                Id = order.Id,
                Date = order.Date,
                OrderLines = order.OrderLines.Select(ol => new OrderLine()
                {
                    Id = ol.Id,
                    ProductId = ol.ProductId,
                    Quantity = ol.Quantity
                }).ToList(),
            });
            return await select.ToListAsync();
        }

        async Task IRepository<Order>.Remove(int id)
        {
            var order = await db.Orders.FirstOrDefaultAsync(p => p.Id == id);
            db.Orders.Remove(order);
            await db.SaveChangesAsync();
        }
    }
}
