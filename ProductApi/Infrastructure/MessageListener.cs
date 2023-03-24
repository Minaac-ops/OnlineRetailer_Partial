using System;
using System.Collections.Generic;
using System.Threading;
using EasyNetQ;
using Microsoft.Extensions.DependencyInjection;
using ProductApi.Data;
using ProductApi.Models;
using Shared;

namespace ProductApi.Infrastructure
{
    public class MessageListener
    {
        IServiceProvider provider;
        string connectionString;
        IBus bus;

        // The service provider is passed as a parameter, because the class needs
        // access to the product repository. With the service provider, we can create
        // a service scope that can provide an instance of the product repository.
        public MessageListener(IServiceProvider provider, string connectionString)
        {
            this.provider = provider;
            this.connectionString = connectionString;
        }

        public void Start()
        {
            using (bus = RabbitHutch.CreateBus(connectionString))
            {
                bus.PubSub.Subscribe<OrderCreatedMessage>("newOrderCheck",
                    HandleOrderCompleted);

                Console.WriteLine("subscribing to newordercheck");
                bus.PubSub.Subscribe<CreditStandingChangedMessage>("orderDelievered", HandleOrderDelivered);
                Console.WriteLine("subscribing to orderdelivered");
                bus.PubSub.Subscribe<OrderStatusChangedMessage>("orderCancelled", HandleOrderCancelled,
                    x => x.WithTopic("cancelled"));
                Console.WriteLine("subscribing to ordercancelled");
                // Add code to subscribe to other OrderStatusChanged events:
                // * cancelled
                // * shipped
                // * paid
                // Implement an event handler for each of these events.
                // Be careful that each subscribe has a unique subscription id
                // (this is the first parameter to the Subscribe method). If they
                // get the same subscription id, they will listen on the same
                // queue.

                // Block the thread so that it will not exit and stop subscribing.
                lock (this)
                {
                    Monitor.Wait(this);
                }
            }

        }

        private void HandleOrderCancelled(OrderStatusChangedMessage obj)
        {
            using var scope = provider.CreateScope();
            var services = scope.ServiceProvider;
            var repo = services.GetService<IRepository<Product>>();

            foreach (var orderLine in obj.OrderLine)
            {
                var product = repo.Get(orderLine.ProductId);
                var result = product.Result;
                result.ItemsReserved -= orderLine.Quantity;
                repo.Edit(orderLine.ProductId, result);
            }
        }

        private void HandleOrderDelivered(CreditStandingChangedMessage obj)
        {
            
        }
        private void HandleOrderCompleted(OrderCreatedMessage message)
        {
            Console.WriteLine("hej handleordercompleted");
            // A service scope is created to get an instance of the product repository.
            // When the service scope is disposed, the product repository instance will
            // also be disposed.
            using var scope = provider.CreateScope();
            var services = scope.ServiceProvider;
            var productRepos = services.GetService<IRepository<Product>>();

            // Reserve items of ordered product (should be a single transaction).
            // Beware that this operation is not idempotent.

            if (ProductItemsAvailable(message.OrderLines,productRepos))
            {
                foreach (var orderLine in message.OrderLines)
                {
                    var product = productRepos.Get(orderLine.ProductId);
                    var result = product.Result;
                    result.ItemsReserved += orderLine.Quantity;
                    productRepos.Edit(orderLine.ProductId, result);
                }

                var replyMessage = new OrderAcceptedMessage
                {
                    OrderId = message.OrderId
                };
                    
                bus.PubSub.Publish(replyMessage);
            }
            else
            {
                // publish an OrderRejectedMessage
                var replyMessage = new OrderRejectedMessage()
                {
                    OrderId = message.OrderId
                };
                bus.PubSub.Publish(replyMessage);
            }
        }
        
        
        private bool ProductItemsAvailable(IList<OrderLine> orderLines, IRepository<Product> productRepos)
        {
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