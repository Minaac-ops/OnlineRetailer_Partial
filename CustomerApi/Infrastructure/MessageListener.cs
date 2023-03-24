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
                bus.PubSub.Subscribe<CreditStandingChangedMessage>("creditChanged",HandleChangeCreditStanding,x => x.WithTopic("paid"));
                bus.PubSub.Subscribe<OrderCreatedMessage>("checkCreditStanding", HandleCheckCreditStanding);

                lock (this)
                {
                    Monitor.Wait(this);
                }
            }
        }

        private void HandleCheckCreditStanding(OrderCreatedMessage obj)
        {
            using var scope = _provider.CreateScope();
            var service = scope.ServiceProvider;
            var repo = service.GetService<IRepository<Customer>>();
            
            var customer = repo.Get(obj.CustomerId).Result;

            if (customer.CreditStanding = true)
            {
                var replyMessage = new OrderRejectedMessage
                {
                    OrderId = obj.OrderId
                };
                bus.PubSub.Publish(replyMessage);
            }
        }

        private void HandleChangeCreditStanding(CreditStandingChangedMessage obj)
        {
            using var scope = _provider.CreateScope();
            var service = scope.ServiceProvider;
            var repo = service.GetService<IRepository<Customer>>();

            repo.ConfirmDelivered(obj.CustomerId);
        }
    }
}