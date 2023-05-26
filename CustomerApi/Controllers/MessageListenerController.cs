using System;
using System.Threading.Tasks;
using CustomerApi.Data;
using CustomerApi.Models;
using Dapr;
using Dapr.Client;
using Microsoft.AspNetCore.Mvc;
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
            Console.WriteLine("CustomerController received " + msg.CustomerId);
            var customer = await _repository.Get(msg.CustomerId);
            customer.CreditStanding = true;
            await _repository.Edit(customer.Id, customer);
        }

        [Topic("orderpubsub", "checkCredit")]
        [HttpPost("/checkCredit")]
        public async Task HandleCheckCreditStanding([FromBody] OrderCreatedMessage msg)
        {
            Console.WriteLine("CUSTOMERLISTENER RECEIVED DAPRMESSAGE. CUSTOMERID: " + msg.CustomerId + " CREATEORDER orderId " + msg.OrderId);
            var customer = await _repository.Get(msg.CustomerId);
            Console.WriteLine("CUSTOMERLISTENER checking creditStanding. CREATEORDER " + customer.CreditStanding);
            using var daprClient = new DaprClientBuilder().Build();
            if (customer.CreditStanding)
            {
                await _repository?.Edit(customer.Id, customer);
                var orderAcceptedMessage = new OrderAcceptedMessage
                {
                    OrderId = msg.OrderId,
                    CustomerId = msg.CustomerId
                };
                await daprClient.PublishEventAsync("orderpubsub", "orderAccepted", orderAcceptedMessage);
                Console.WriteLine("CUSTOMERLISTENER. PUBLISHED ORDERACCEPTED with OrderId " + orderAcceptedMessage.OrderId);
            } else
            {
                var orderRejectedMessage = new OrderRejectedMessage
                {
                    OrderId = msg.OrderId,
                };
                
                //await bus.PubSub.PublishAsync(orderRejectedMessage);
                await daprClient.PublishEventAsync("orderpubsub", "orderRejected", orderRejectedMessage);
                Console.WriteLine("CUSTOMERLISTENER PUBLISHED ORDERREJECTED WITH ORDERID: " + orderRejectedMessage.OrderId);
            }
            customer.CreditStanding = false;
            await _repository.Edit(customer.Id,customer);
        }
    }
}
