using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using OrderApi.Data;
using OrderApi.Infrastructure;
using RestSharp;
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

        public OrdersController(IRepository<Order> repos,
                                IServiceGateway<ProductDto> gateway,
                                IMessagePublisher publisher)
        {
            repository = repos;
            productServiceGateway = gateway;
            _messagePublisher = publisher;
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
        public async Task<Order> Post([FromBody]Order order)
        {
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
                        order.Status = Order.OrderStatus.Completed;
                        var newOrder = repository.Add(order);
                        return await newOrder;
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e);
                        throw;
                    }
                }
                return order;
        }

        private bool ProductItemsAvailable(Order order)
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
