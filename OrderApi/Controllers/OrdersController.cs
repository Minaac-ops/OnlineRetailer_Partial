using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Microsoft.AspNetCore.Mvc;
using OrderApi.Data;
using OrderApi.Infrastructure;
using OrderApi.Models;
using Shared;

namespace OrderApi.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class OrdersController : ControllerBase
    {
        private readonly IRepository<Order> repository;
        private IMessagePublisher _messagePublisher;
        private IConverter<Order,OrderDto> _converter;

        public OrdersController(IRepository<Order> repos,
                                IMessagePublisher publisher,
                                IConverter<Order,OrderDto> orderConverter)
        {
            repository = repos;
            _messagePublisher = publisher;
            _converter = orderConverter;
        }

        // GET: orders
        [HttpGet]
        public IEnumerable<OrderDto> Get()
        {
            try
            {
                var orders = repository.GetAll();
                var dtos = orders.Select(o => new OrderDto
                {
                    Id = o.Id,
                    CustomerId = o.CustomerId,
                    Date = o.Date,
                    OrderLines = o.OrderLines,
                    Status = o.Status
                });
                return dtos;
            }
            catch (Exception e)
            {
                throw new Exception("Orders couldn't be displayed due to error " + e.Message);
            }
            
        }

        [HttpGet("getByCustomer/{customerId}")]
        public IEnumerable<OrderDto> GetByCustomerId(int customerId)
        {
            try
            {
                var items = repository.GetByCustomerId(customerId);
                var dtos = items.Select(o => new OrderDto()
                {
                    Id = o.Id,
                    CustomerId = o.CustomerId,
                    Date = o.Date,
                    OrderLines = o.OrderLines,
                    Status = o.Status
                });
                return dtos;
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
        }

        // GET orders/5
        [HttpGet("{id}", Name = "GetOrder")]
        public IActionResult Get(int id)
        {
            try
            {
                var item = repository.Get(id);
                return new ObjectResult(item);
            }
            catch (Exception e)
            {
                throw new Exception("Order with id "+id+" couldn't be displayed due to error "+e.Message);
            }
        }

        // POST orders
        [HttpPost]
        public IActionResult Post([FromBody]OrderDto order)
        {
            //Checking if order is null
            if (order == null) throw new Exception("Fill out order details.");

            try
            {
                order.Status = OrderStatus.Tentative;
                var newOrder = repository.Add(_converter.Convert(order));
                
                //publish orderstatuschanged

                Console.WriteLine("before published");
                _messagePublisher.PublishOrderCreatedMessage(newOrder.CustomerId, newOrder.Id, newOrder.OrderLines);
                
                Console.WriteLine("published");
                
                //wait until order status is "completed"
                bool isCompleted = false;
                while (!isCompleted)
                {
                    var tentativeOrder = repository.Get(newOrder.Id);
                    if (tentativeOrder.Status == OrderStatus.Completed) isCompleted = true;
                    Thread.Sleep(1000);
                    
                    Console.WriteLine("orderstatus changed");
                }
                return CreatedAtRoute("GetOrder", new { id = newOrder.Id }, newOrder);
            }
            catch (Exception e)
            {
                throw new Exception(e.Message);
            }
        }

        // PUT orders/5/ship
        // This action method ships an order and publishes an OrderStatusChangedMessage.
        // with topic set to "shipped".
        [HttpPut("{id}/ship")]
        public IActionResult Ship(int id)
        {
            try
            {
                repository.Edit(id, new Order
                {
                    Id = id,
                    Status = OrderStatus.Shipped
                });
                var order = repository.Get(id);
                _messagePublisher.OrderStatusChangedMessage(id,order.OrderLines,"shipped");
                return Ok();
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
        }

        // PUT orders/5/pay
        // This action method marks an order as paid and publishes a CreditStandingChangedMessage
        // (which have not yet been implemented), if the credit standing changes.
        [HttpPut("{id}/pay")]
        public IActionResult Pay(int id)
        {
            try
            {
                var order = repository.Get(id);

                repository.Edit(id, new Order()
                {
                    Id = id,
                    Status = OrderStatus.Paid
                });
                _messagePublisher.CreditStandingChangedMessage(order.CustomerId);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
            // Add code to implement this method.
            return Ok();
        }
        
        // PUT orders/5/cancel
        // This action method cancels an order and publishes an OrderStatusChangedMessage
        // with topic set to "cancelled".
        [HttpPut("{id}/cancel")]
        public IActionResult Cancel(int id)
        {
            try
            {
                repository.Edit(id, new Order
                {
                    Id = id,
                    Status = OrderStatus.Cancelled
                });
                var order = repository.Get(id);
                _messagePublisher.OrderStatusChangedMessage(id,order.OrderLines,"cancelled");
                return Ok();
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
        }

    }
}
