using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
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
        private IConverter<Order, OrderDto> _converter;

        public OrdersController(IRepository<Order> repos,
            IMessagePublisher publisher,
            IConverter<Order, OrderDto> orderConverter)
        {
            repository = repos;
            _messagePublisher = publisher;
            _converter = orderConverter;
        }

        // GET: orders
        [HttpGet]
        public async Task<IEnumerable<OrderDto>> Get()
        {
            try
            {
                var orders = await repository.GetAll();
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
        public async Task<IEnumerable<OrderDto>> GetByCustomerId(int customerId)
        {
            try
            {
                var items = await repository.GetByCustomerId(customerId);
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
        public async Task<OrderDto> Get(int id)
        {
            try
            {
                var item = await repository.Get(id);
                return _converter.Convert(item);
            }
            catch (Exception e)
            {
                throw new Exception("Order with id " + id + " couldn't be displayed due to error " + e.Message);
            }
        }

        // POST orders
        [HttpPost]
        public async Task<OrderDto> Post([FromBody] OrderDto order)
        {
            //Checking if order is null
            if (order == null) throw new Exception("Fill out order details.");

            try
            {
                order.Status = OrderDto.OrderStatus.Tentative;
                Console.WriteLine("OrderController before add should be tentative: "+order.Status.ToString());
                var newOrder = await repository.Add(_converter.Convert(order));
                Console.WriteLine("OrderController after add should be tentative: "+order.Status.ToString());

                //publish orderstatuschanged

                Console.WriteLine("before published");
                _messagePublisher.PublishOrderCreatedMessage(newOrder.CustomerId, newOrder.Id, newOrder.OrderLines);
                
                // Wait until order status is "completed"
                bool isCompleted = false;
                while (!isCompleted)
                {
                    Thread.Sleep(5000);
                    var tentativeOrder = await repository.Get(newOrder.Id);
                    if (tentativeOrder.Status == OrderDto.OrderStatus.Completed)
                    {
                        isCompleted = true;
                        Thread.Sleep(1000);
                    }
                }
                Console.WriteLine("right before return status should be "+ newOrder.Status);
                return _converter.Convert(newOrder);
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
        public async void Ship(int id)
        {
            try
            {
                var order = await repository.Get(id);
                order.Status = OrderDto.OrderStatus.Shipped;
                repository.Edit(order);
                
                _messagePublisher.OrderStatusChangedMessage(id, order.OrderLines, "shipped");
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
        public async void Pay(int id)
        {
            try
            {
                var order = await repository.Get(id);
                order.Status = OrderDto.OrderStatus.Paid;
                repository.Edit(order);
                
                _messagePublisher.CreditStandingChangedMessage(order.CustomerId);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }

            // Add code to implement this method.
        }

        // PUT orders/5/cancel
        // This action method cancels an order and publishes an OrderStatusChangedMessage
        // with topic set to "cancelled".
        [HttpPut("{id}/cancel")]
        public async void Cancel(int id)
        {
            try
            {
                var order = await repository.Get(id);
                switch (order.Status)
                {
                    case OrderDto.OrderStatus.Shipped:
                        throw new Exception("Can't cancel an order that has already been shipped. Call 911.");
                        break;
                    case OrderDto.OrderStatus.Cancelled:
                        throw new Exception("Order already cancelled");
                        break;
                    case OrderDto.OrderStatus.Completed:
                        order.Status = OrderDto.OrderStatus.Cancelled;
                        repository.Edit(order);
                        _messagePublisher.CreditStandingChangedMessage(order.CustomerId);
                        _messagePublisher.OrderStatusChangedMessage(id, order.OrderLines,"cancelled");
                        break;
                    case OrderDto.OrderStatus.Paid:
                        order.Status = OrderDto.OrderStatus.Cancelled;
                        repository.Edit(order);
                        _messagePublisher.OrderStatusChangedMessage(id,order.OrderLines,"cancelled");
                        break;
                    case OrderDto.OrderStatus.Tentative:
                        order.Status = OrderDto.OrderStatus.Cancelled;
                        repository.Edit(order);
                        _messagePublisher.OrderStatusChangedMessage(id,order.OrderLines,"cancelled");
                        break;
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
        }
    }
}