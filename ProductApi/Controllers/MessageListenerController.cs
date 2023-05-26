using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Dapr;
using Dapr.Client;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.CodeAnalysis.Elfie.Serialization;
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
            Console.WriteLine("ProductListener HandleProductCheck");

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
                Console.WriteLine("ProductListener PublishOrderAcceptedMessage");
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
                Console.WriteLine("ProductListener PublishOrderRejectedMessage");
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
