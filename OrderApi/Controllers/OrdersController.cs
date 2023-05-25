using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Monitoring;
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
            using var activity = MonitorService.ActivitySource.StartActivity();
            try
            {
                MonitorService.Log.Debug("OrdersController Get");
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
            using var activity = MonitorService.ActivitySource.StartActivity();
            MonitorService.Log.Here().Debug("OrdersController POST");
            //Checking if order is null
            if (order == null) throw new Exception("Fill out order details.");
            try
            {
                order.Status = OrderDto.OrderStatus.Tentative;
                var newOrder = await repository.Add(_converter.Convert(order));

                //publish orderstatuschanged

                await _messagePublisher.PublishOrderCreatedMessage(newOrder.CustomerId, newOrder.Id, newOrder.OrderLines);
                
                // Wait until order status is "completed"
                bool isCompleted = false;
                while (!isCompleted)
                {
                    Thread.Sleep(2000);
                    var tentativeOrder = await repository.Get(newOrder.Id);
                    if (tentativeOrder.Status == OrderDto.OrderStatus.Completed)
                    {
                        isCompleted = true;
                        Thread.Sleep(1000);
                        await _messagePublisher.PublishOrderAccepted(order.CustomerId, order.Id);
                    }
                }
                return _converter.Convert(newOrder);
            }
            catch (Exception e)
            {
                await _messagePublisher.PublishOrderCancelled(order.CustomerId,order.Id);
                throw new Exception(e.Message);
            }
        }

        // PUT orders/5/ship
        // This action method ships an order and publishes an OrderStatusChangedMessage.
        // with topic set to "shipped".
        [HttpPut("{id}/ship")]
        public async Task Ship(int id)
        { 
            using var activity = MonitorService.ActivitySource.StartActivity();
            MonitorService.Log.Here().Debug("Entered OrderController Pay");
            try
            {
                var order = await repository.Get(id);
                order.Status = OrderDto.OrderStatus.Shipped;
                await repository.Edit(order);
                
                await _messagePublisher.OrderStatusChangedMessage(id, order.OrderLines, "shipped");
                await _messagePublisher.PublishOrderShippedEmail(order.CustomerId, id);
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
        public async Task Pay(int id)
        {
            try
            {
                var order = await repository.Get(id);
                order.Status = OrderDto.OrderStatus.Paid;
                await repository.Edit(order);
                
                await _messagePublisher.CreditStandingChangedMessage(order.CustomerId);
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
        public async Task Cancel(int id)
        {
            using var activity = MonitorService.ActivitySource.StartActivity();
            MonitorService.Log.Here().Debug("Entered OrderController Cancel");
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
                        await repository.Edit(order);
                        await _messagePublisher.CreditStandingChangedMessage(order.CustomerId);
                        await _messagePublisher.OrderStatusChangedMessage(id, order.OrderLines,"cancelled");
                        await _messagePublisher.PublishOrderCancelled(order.CustomerId,order.Id);
                        break;
                    case OrderDto.OrderStatus.Paid:
                        order.Status = OrderDto.OrderStatus.Cancelled;
                        await repository.Edit(order);
                        await _messagePublisher.OrderStatusChangedMessage(id,order.OrderLines,"cancelled");
                        await _messagePublisher.PublishOrderCancelled(order.CustomerId,order.Id);
                        break;
                    case OrderDto.OrderStatus.Tentative:
                        order.Status = OrderDto.OrderStatus.Cancelled;
                        await repository.Edit(order);
                        await _messagePublisher.OrderStatusChangedMessage(id,order.OrderLines,"cancelled");
                        await _messagePublisher.PublishOrderCancelled(order.CustomerId,order.Id);
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