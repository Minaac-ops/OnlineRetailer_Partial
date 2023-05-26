using System;
using System.Threading.Tasks;
using Dapr;
using Microsoft.AspNetCore.Mvc;
using Monitoring;
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
            MonitorService.Log.Here().Debug("OrderApi: MessageListener HandleOrderRejected");

            //Delete order
            var order = await _repository.Get(msg.OrderId);

            order.Status = OrderDto.OrderStatus.Cancelled;
            await _repository.Edit(order);
        }
    }
}
