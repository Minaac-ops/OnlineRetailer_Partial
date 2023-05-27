using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using Dapr;
using Microsoft.AspNetCore.Mvc;
using Monitoring;
using OpenTelemetry;
using OpenTelemetry.Context.Propagation;
using OrderApi.Data;
using OrderApi.Models;
using Shared;

namespace OrderApi.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class MessageListenerController : ControllerBase
    {
        private readonly IRepository<Order> _repository;

        public MessageListenerController(IRepository<Order> repository)
        {
            _repository = repository;
        }

        [Topic("orderpubsub", "orderAccepted")]
        [HttpPost("/orderAccepted")]
        public async Task HandleOrderAccepted([FromBody] OrderAcceptedMessage msg)
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
            
            MonitorService.Log.Here().Debug("OrderApi: MessageListener HandleOrderAccepted");

            //Mark order as completed
            var order = await _repository.Get(msg.OrderId);
            
            order.Status = OrderDto.OrderStatus.Completed;
            
            await _repository.Edit(order);
        }
        
        [Topic("orderpubsub", "orderRejected")]
        [HttpPost("/orderRejected")]
        public async Task HandleOrderRejected([FromBody] OrderRejectedMessage msg)
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
            
            MonitorService.Log.Here().Debug("OrderApi: MessageListener HandleOrderRejected");

            //Delete order
            var order = await _repository.Get(msg.OrderId);

            order.Status = OrderDto.OrderStatus.Cancelled;
            await _repository.Edit(order);
        }
    }
}
