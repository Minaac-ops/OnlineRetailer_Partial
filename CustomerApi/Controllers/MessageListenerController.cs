using System;
using System.Threading.Tasks;
using CustomerApi.Data;
using CustomerApi.Models;
using Dapr;
using Dapr.Client;
using Microsoft.AspNetCore.Mvc;
using Monitoring;
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
                    await daprClient.PublishEventAsync("orderpubsub", "orderAccepted", orderAcceptedMessage);
                    MonitorService.Log.Here().Debug("CustomerApi: MessageListener Published OrderAcceptedMessage");
                } else
                {
                    var orderRejectedMessage = new OrderRejectedMessage
                    {
                        OrderId = msg.OrderId,
                    };
                
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
