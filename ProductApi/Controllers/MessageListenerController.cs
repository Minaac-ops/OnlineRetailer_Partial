using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Dapr;
using Dapr.Client;
using Microsoft.AspNetCore.Mvc;
using Monitoring;
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
            MonitorService.Log.Here().Debug("ProductApi: MessageListener HandleProductCheck");

            using var daprClient = new DaprClientBuilder().Build();
            if (ProductItemsAvailable(msg.OrderLines,_repository))
            {
                foreach (var orderLine in msg.OrderLines)
                {
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
                
                await daprClient.PublishEventAsync("orderpubsub", "orderAccepted", orderAcceptedMessage);
                MonitorService.Log.Here().Debug("ProductApi: MessageListener published OrderAcceptedMessage");
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
                MonitorService.Log.Here().Debug("ProductApi: MessageListener published OrderRejectedMessage");
            }
        }

        [Topic("orderpubsub", "shipped")]
        [HttpPost("/productsShipped")]
        public async Task HandleOrderShipped([FromBody] OrderStatusChangedMessage msg)
        {
            MonitorService.Log.Here().Debug("ProductApi: MessageListener HandleOrderShipped");
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
            MonitorService.Log.Here().Debug("ProductApi: MessageListener HandleOrderCancelled");
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
            MonitorService.Log.Here().Debug("ProductApi: MessageListener ProductItemsAvailable");
            foreach (var orderLine in orderLines)
            {
                var product = productRepos.Get(orderLine.ProductId);
                var result = product.Result;
                if (orderLine.Quantity > result.ItemsInStock - result.ItemsReserved)
                {
                    return false;
                }
            }
            return true;
        }
    }
}
