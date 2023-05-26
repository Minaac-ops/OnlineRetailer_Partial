using System;
using System.Threading.Tasks;
using Dapr;
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
            Console.WriteLine("ORDERLISTENER RECEIVED DAPR ORDER ACCEPTED customerId: " + msg.CustomerId
                +". orderId: "+ msg.OrderId);

            //Mark order as completed
            var order = await _repository.Get(msg.OrderId);
            
            order.Status = OrderDto.OrderStatus.Completed;
            
            await _repository.Edit(order);
        }
        
        [Topic("orderpubsub", "orderRejected")]
        [HttpPost("/orderRejected")]
        public async Task HandleOrderRejected([FromBody] OrderRejectedMessage msg)
        {
            Console.WriteLine("ORDERLISTENER RECEIVED DAPR ORDER RECEJTED orderId: "+ msg.OrderId);

            //Delete order
            var order = await _repository.Get(msg.OrderId);

            order.Status = OrderDto.OrderStatus.Cancelled;
            await _repository.Edit(order);
        }
    }
}
