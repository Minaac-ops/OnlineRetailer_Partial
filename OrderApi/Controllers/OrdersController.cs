using System;
using System.Collections.Generic;
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
        private IServiceGateway<ProductDto> productServiceGateway;
        private IMessagePublisher _messagePublisher;
        private IConverter<Order,OrderDto> _converter;

        public OrdersController(IRepository<Order> repos,
                                IServiceGateway<ProductDto> gateway,
                                IMessagePublisher publisher,
                                IConverter<Order,OrderDto> orderConverter)
        {
            repository = repos;
            productServiceGateway = gateway;
            _messagePublisher = publisher;
            _converter = orderConverter;
        }

        // GET: orders
        [HttpGet]
        public async Task<IEnumerable<Order>> Get()
        {
            try
            {
                return await repository.GetAll();
            }
            catch (Exception e)
            {
                throw new Exception("Orders couldn't be displayed due to error " + e.Message);
            }
            
        }

        // GET orders/5
        [HttpGet("{id}", Name = "GetOrder")]
        public async Task<Order> Get(int id)
        {
            try
            {
                var item = await repository.Get(id);
                return item;
            }
            catch (Exception e)
            {
                throw new Exception("Order with id "+id+" couldn't be displayed due to error "+e.Message);
            }
            
        }

        // POST orders
        [HttpPost]
        public async Task<OrderDto> Post([FromBody]OrderDto order)
        {
            foreach (var orderline in order.OrderLines)
            {
                Console.WriteLine("orderlind før gemt " + orderline.OrderId);
            }
            Console.WriteLine(order.CustomerId);
            //Checking if order is null
            
                if (order == null) throw new Exception("Fill out order details.");

                if (ProductItemsAvailable(order))
                {
                    try
                    {
                        //Publish OrderStatusChangedMessage. If this operation fails, the order
                        //will not be created
                        _messagePublisher.PublishOrderStatusChangedMessage(
                            order.CustomerId,order.OrderLines,"completed");
                        
                        //Create order
                        order.Status = OrderStatus.Completed;
                        var newOrder = await repository.Add(_converter.Convert(order));
                        foreach (var VARIABLE in newOrder.OrderLines)
                        {
                            Console.WriteLine("efter det er gemt " + VARIABLE.OrderId+VARIABLE.ProductId);
                        }
                        return _converter.Convert(newOrder);
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e);
                        throw;
                    }
                }
                return order;
        }

        private bool ProductItemsAvailable(OrderDto order)
        {
            foreach (var orderline in order.OrderLines)
            {
                //call productService to get the product ordered
                var orderedProduct = productServiceGateway.Get(orderline.ProductId);
                if (orderline.Quantity > orderedProduct.ItemsInStock - orderedProduct.ItemsReserved)
                {
                    return false;
                }
            }
            return true;
        }

    }
}
