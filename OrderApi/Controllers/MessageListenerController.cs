using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Dapr;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
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
            Console.WriteLine("ReceivedOrderAccepted");

            //Mark order as completed
            var order = await _repository.Get(msg.OrderId);
            Console.WriteLine("handleOrderAccepted orderstatus before edit "+order.Status + "orderid "+msg.OrderId);
            order.Status = OrderDto.OrderStatus.Completed;
            Console.WriteLine("handleOrderAccepted orderstaus after edit "+ order.Status);
            await _repository.Edit(order);
            var newOrder = await _repository.Get(msg.OrderId);
            Console.WriteLine("after save: "+newOrder.Status);
            Console.WriteLine("HandleOrderAccepted");
        }
        
        [Topic("orderpubsub", "orderRejected")]
        [HttpPost("/orderRejected")]
        public async Task HandleOrderRejected([FromBody] OrderRejectedMessage msg)
        {
            Console.WriteLine("Received OrderRejectedMessage");

            //Delete order
            var order = await _repository.Get(msg.OrderId);

            order.Status = OrderDto.OrderStatus.Cancelled;
            await _repository.Edit(order);
            Console.WriteLine("Handled OrderRejectedMessage");
        }
    }
}
