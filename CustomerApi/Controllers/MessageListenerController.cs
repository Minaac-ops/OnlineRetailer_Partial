using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using CustomerApi.Data;
using CustomerApi.Models;
using Dapr;
using Dapr.Client;
using Microsoft.AspNetCore.Mvc;
using Monitoring;
using OpenTelemetry;
using OpenTelemetry.Context.Propagation;
using Shared;

namespace CustomerApi.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class MessageListenerController : ControllerBase
    {
        private readonly IRepository<Customer> _repository;

        public MessageListenerController(IRepository<Customer> repository)
        {
            _repository = repository;
        }
        
        [Topic("orderpubsub", "creditChange")]
        [HttpPost("/creditChange")]
        public async Task HandleCreditStatusChanged([FromBody] CreditStandingChangedMessage msg)
        {
            MonitorService.Log.Here().Debug("CustomerApi: MessageListener HandleCreditStatusChanged");
            try
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
                
                var customer = await _repository.Get(msg.CustomerId);
                customer.CreditStanding = true;
                await _repository.Edit(customer.Id, customer);
            }
            catch (Exception e)
            {
                throw new Exception(e.Message);
            }
        }

        [Topic("orderpubsub", "checkCredit")]
        [HttpPost("/checkCredit")]
        public async Task HandleCheckCreditStanding([FromBody] OrderCreatedMessage msg)
        {
            MonitorService.Log.Here().Debug("CustomerApi: MessageListener HandleCheckCreditStanding");
            try
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
                
                var customer = await _repository.Get(msg.CustomerId);
            
                using var daprClient = new DaprClientBuilder().Build();
                if (customer.CreditStanding)
                {
                    await _repository?.Edit(customer.Id, customer);
                    var orderAcceptedMessage = new OrderAcceptedMessage
                    {
                        OrderId = msg.OrderId,
                        CustomerId = msg.CustomerId
                    };
                    
                    // Adding header to the message so the activity can continue in emailService
                    propagator.Inject(parentCtx, orderAcceptedMessage, (r, key, value) =>
                    {
                        r.Header.Add(key, value);
                    });
                    
                    await daprClient.PublishEventAsync("orderpubsub", "orderAccepted", orderAcceptedMessage);
                    MonitorService.Log.Here().Debug("CustomerApi: MessageListener Published OrderAcceptedMessage");
                } else
                {
                    var orderRejectedMessage = new OrderRejectedMessage
                    {
                        OrderId = msg.OrderId,
                    };
                    
                    // Adding header to the message so the activity can continue in emailService
                    propagator.Inject(parentCtx, orderRejectedMessage, (r, key, value) =>
                    {
                        r.Header.Add(key, value);
                    });
                
                    //await bus.PubSub.PublishAsync(orderRejectedMessage);
                    await daprClient.PublishEventAsync("orderpubsub", "orderRejected", orderRejectedMessage);
                    MonitorService.Log.Here().Debug("CustomerApi: MessageListener Published OrderRejectedMessage");
                }
                customer.CreditStanding = false;
                await _repository.Edit(customer.Id,customer);
            }
            catch (Exception e)
            {
                throw new Exception(e.Message);
            }
        }
    }
}
