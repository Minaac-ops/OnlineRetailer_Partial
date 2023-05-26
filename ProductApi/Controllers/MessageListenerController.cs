using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Dapr;
using Dapr.Client;
using Microsoft.AspNetCore.Mvc;
using ProductApi.Data;
using ProductApi.Models;
using Shared;

namespace ProductApi.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class MessageListenerController : ControllerBase
    {
        private readonly IRepository<Product> _repository;

        public MessageListenerController(IRepository<Product> repository)
        {
            _repository = repository;
        }

        [Topic("orderpubsub", "checkProductAvailability")]
        [HttpPost("/checkProductAvailability")]
        public async Task HandleProductCheck([FromBody] OrderCreatedMessage msg)
        {
            Console.WriteLine("PRODUCT LISTENER RECEIVED DAPR MESSAGE WITH ORDERID: "+msg.OrderId+ " AND PRODUCTS " );
            foreach (var VARIABLE in msg.OrderLines)
            {
                Console.WriteLine(VARIABLE.ProductId);
            }

            // Reserve items of ordered product (should be a single transaction).
            // Beware that this operation is not idempotent.
            using var daprClient = new DaprClientBuilder().Build();
            if (ProductItemsAvailable(msg.OrderLines,_repository))
            {
                foreach (var orderLine in msg.OrderLines)
                {
                    Console.WriteLine(orderLine.Quantity);
                    var product =await _repository.Get(orderLine.ProductId);
                    product.ItemsReserved += orderLine.Quantity;
                    product.ItemsInStock -= orderLine.Quantity;
                    await _repository.Edit(product);
                }

                var orderAcceptedMessage = new OrderAcceptedMessage
                {
                    OrderId = msg.OrderId,
                    CustomerId = msg.CustomerId,
                };
                    
                //await _bus.PubSub.PublishAsync(replyMessage);
                await daprClient.PublishEventAsync("orderpubsub", "orderAccepted", orderAcceptedMessage);
                Console.WriteLine("PRODUCTLISTENER PUBLISHED ORDERACCEPTED WITH " + orderAcceptedMessage.OrderId);
            }
            else
            {
                // publish an OrderRejectedMessage
                var orderRejectedMessage = new OrderRejectedMessage()
                {
                    OrderId = msg.OrderId
                };
                //await _bus.PubSub.PublishAsync(replyMessage);
                await daprClient.PublishEventAsync("orderpubsub", "orderRejected", orderRejectedMessage);
                Console.WriteLine("PRODUCTLISTENER PUBLISHED ORDERREJECTED WITH " + orderRejectedMessage.OrderId);
            }
        }

        [Topic("orderpubsub", "shipped")]
        [HttpPost("/productsShipped")]
        public async Task HandleOrderShipped([FromBody] OrderStatusChangedMessage msg)
        {
            foreach (var orderLine in msg.OrderLine)
            {
                var p = await _repository.Get(orderLine.ProductId);
                p.ItemsReserved -= orderLine.Quantity;
                await _repository.Edit(p);
            }
        }
        
        [Topic("orderpubsub", "cancelled")]
        [HttpPost("/orderCancelled")]
        public async Task HandleOrderCancelled([FromBody] OrderStatusChangedMessage msg)
        {
            foreach (var orderLine in msg.OrderLine)
            {
                var product = await _repository.Get(orderLine.ProductId);
                product.ItemsReserved -= orderLine.Quantity;
                product.ItemsInStock += orderLine.Quantity;
                await _repository.Edit(product);
            }
        }
        
        
        private bool ProductItemsAvailable(IList<OrderLine> orderLines, IRepository<Product> productRepos)
        {
            foreach (var orderLine in orderLines)
            {
                var product = productRepos.Get(orderLine.ProductId);
                var result = product.Result;
                if (orderLine.Quantity > result.ItemsInStock - result.ItemsReserved)
                {
                    Console.WriteLine("ProductListener Not enough products");
                    return false;
                }
            }
            return true;
        }
    }
}
