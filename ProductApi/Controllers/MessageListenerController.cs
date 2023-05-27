using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using Dapr;
using Dapr.Client;
using Microsoft.AspNetCore.Mvc;
using Monitoring;
using OpenTelemetry;
using OpenTelemetry.Context.Propagation;
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
            var propagator = new TraceContextPropagator();
            var parentCtx = propagator.Extract(default, msg,
                (r, key) =>
                {
                    return new List<string>(new[]
                        {r.Header.ContainsKey(key) ? r.Header[key].ToString() : string.Empty});
                });
            Baggage.Current = parentCtx.Baggage;
            using var activity = MonitorService.ActivitySource.StartActivity("Message received", ActivityKind.Consumer,
                parentCtx.ActivityContext);
            
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
                
                // Adding header to the message so the activity can continue in emailService
                propagator.Inject(parentCtx, orderAcceptedMessage, (r, key, value) =>
                {
                    r.Header.Add(key, value);
                });
                
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
                
                // Adding header to the message so the activity can continue in emailService
                propagator.Inject(parentCtx, orderRejectedMessage, (r, key, value) =>
                {
                    r.Header.Add(key, value);
                });
                
                //await _bus.PubSub.PublishAsync(replyMessage);
                await daprClient.PublishEventAsync("orderpubsub", "orderRejected", orderRejectedMessage);
                MonitorService.Log.Here().Debug("ProductApi: MessageListener published OrderRejectedMessage");
            }
        }

        [Topic("orderpubsub", "shipped")]
        [HttpPost("/productsShipped")]
        public async Task HandleOrderShipped([FromBody] OrderStatusChangedMessage msg)
        {
            var propagator = new TraceContextPropagator();
            var parentCtx = propagator.Extract(default, msg,
                (r, key) =>
                {
                    return new List<string>(new[]
                        {r.Header.ContainsKey(key) ? r.Header[key].ToString() : string.Empty});
                });
            Baggage.Current = parentCtx.Baggage;
            using var activity = MonitorService.ActivitySource.StartActivity("Message received", ActivityKind.Consumer,
                parentCtx.ActivityContext);
            
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
            var propagator = new TraceContextPropagator();
            var parentCtx = propagator.Extract(default, msg,
                (r, key) =>
                {
                    return new List<string>(new[]
                        {r.Header.ContainsKey(key) ? r.Header[key].ToString() : string.Empty});
                });
            Baggage.Current = parentCtx.Baggage;
            using var activity = MonitorService.ActivitySource.StartActivity("Message received", ActivityKind.Consumer,
                parentCtx.ActivityContext);
            
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
