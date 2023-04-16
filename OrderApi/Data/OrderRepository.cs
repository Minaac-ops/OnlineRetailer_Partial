using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
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

        Order IRepository<Order>.Add(Order entity)
        {
            Console.WriteLine("OrderRepo: Before adds to db " + entity);
            if (entity == null)
            {
                throw new Exception("An empty order can't be saved to the database.");
            }
            entity.Date ??= DateTime.Now;
            
            var newOrder = db.Orders.Add(entity);
            Console.WriteLine("OrderRepo: After adding to db "+ entity);
            db.SaveChanges();;
            return new Order
            {
                CustomerId = newOrder.Entity.CustomerId,
                Date = newOrder.Entity.Date,
                OrderLines = newOrder.Entity.OrderLines,
                Id = newOrder.Entity.Id,
                Status = newOrder.Entity.Status
            };
        }

        void IRepository<Order>.Edit(Order entity)
        {
            if (entity == null)
            {
                throw new Exception("Couldn't find customer with id "+entity.Id+" to update.");
            }
            Console.WriteLine(entity.Status);
            
            db.Entry(entity).State = EntityState.Modified;
            db.SaveChanges();
        }

        Order IRepository<Order>.Get(int id)
        {
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
            return entity.Result;
        }

        IEnumerable<Order> IRepository<Order>.GetAll()
        {
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
            return select.ToListAsync().Result;
        }

        void IRepository<Order>.Remove(int id)
        {
            var order = db.Orders.FirstOrDefaultAsync(p => p.Id == id);
            db.Orders.Remove(order.Result);
            db.SaveChangesAsync();
        }

        IEnumerable<Order> IRepository<Order>.GetByCustomerId(int customerId)
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
            return entities.Result;
        }
    }
}
