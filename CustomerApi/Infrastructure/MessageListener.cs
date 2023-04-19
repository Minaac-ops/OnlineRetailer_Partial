using System;
using System.Security.Cryptography.Xml;
using System.Threading;
using CustomerApi.Data;
using CustomerApi.Models;
using EasyNetQ;
using Microsoft.Extensions.DependencyInjection;
using Shared;

namespace CustomerApi.Infrastructure
{
    public class MessageListener
    {
        IServiceProvider _provider;
        string connectionString;
        IBus bus;


        public MessageListener(IServiceProvider provider, string connectionString)
        {
            _provider = provider;
            this.connectionString = connectionString;
        }

        public void Start()
        {
            using (bus = RabbitHutch.CreateBus(connectionString))
            {
                bus.PubSub.SubscribeAsync<CreditStandingChangedMessage>
                    ("creditChanged",HandleChangeCreditStanding,x => x.WithTopic("paid"));
                Console.WriteLine("CustomerListener: Listening to CreditStandingChangedMessage");
                
                bus.PubSub.SubscribeAsync<OrderCreatedMessage>
                    ("checkCreditStanding", HandleCheckCreditStanding,x => x.WithTopic("checkCredit"));
                Console.WriteLine("CustomerListener: Listening to OrderCreatedMessage");
                
                lock (this)
                {
                    Monitor.Wait(this);
                }
            }
        }

        private async void HandleCheckCreditStanding(OrderCreatedMessage obj)
        {
            Console.WriteLine("CustomerListener: Received OrderCreatedMessage");
            using var scope = _provider.CreateScope();
            var service = scope.ServiceProvider;
            var repo = service.GetService<IRepository<Customer>>();
            
            var customer = await repo?.Get(obj.CustomerId);
            Console.WriteLine("Customer CreditStanding: "+customer.CreditStanding);
            if (customer.CreditStanding)
            {
                await repo?.Edit(customer.Id, customer);
                var orderAcceptedMessage = new OrderAcceptedMessage
                {
                    OrderId = obj.OrderId
                };
                await bus.PubSub.PublishAsync(orderAcceptedMessage);
                Console.WriteLine("CustomerListener: PublishedOrderAccepted");
            } else
            {
                var orderRejectedMessage = new OrderRejectedMessage
                {
                    OrderId = obj.OrderId
                };
                await bus.PubSub.PublishAsync(orderRejectedMessage);
                Console.WriteLine("CustomerListener: PublishedOrderRejected");
            }
 
            customer.CreditStanding = false;
            await repo.Edit(customer.Id,customer);
        }

        private async void HandleChangeCreditStanding(CreditStandingChangedMessage obj)
        {
            Console.WriteLine("CustomerListener: Received HandleChangedCreditStanding ");
            using var scope = _provider.CreateScope();
            var service = scope.ServiceProvider;
            var repo = service.GetService<IRepository<Customer>>();

            var customer = await repo.Get(obj.CustomerId);
            customer.CreditStanding = true;
            await repo.Edit(customer.Id, customer);
        }
    }
}