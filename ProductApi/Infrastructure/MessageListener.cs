using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
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
        IBus _bus;

        // The service provider is passed as a parameter, because the class needs
        // access to the product repository. With the service provider, we can create
        // a service scope that can provide an instance of the product repository.
        public MessageListener(IServiceProvider provider, string connectionString)
        {
            this.provider = provider;
            this.connectionString = connectionString;
        }

        public async void Start()
        {
            Console.WriteLine("EnteredStart");
            using (_bus = RabbitHutch.CreateBus(connectionString))
            {
                await _bus.PubSub.SubscribeAsync<OrderCreatedMessage>
                ("checkProducts", HandleProductCheck,x=>x.WithTopic("checkProductAvailability"));
                Console.WriteLine("subscribing to newordercheck");
                
                await _bus.PubSub.SubscribeAsync<OrderStatusChangedMessage>("orderCancelled", HandleOrderCancelled,
                    x => x.WithTopic("cancelled"));
                Console.WriteLine("subscribing to ordercancelled");

                await _bus.PubSub.SubscribeAsync<OrderStatusChangedMessage>("OrderShipped", HandleOrderShipped,
                    x => x.WithTopic("shipped"));
                
                lock (this)
                {
                    Monitor.Wait(this);
                }
            }

        }

        private async void HandleOrderShipped(OrderStatusChangedMessage obj)
        {
            using var scope = provider.CreateScope();
            var services = scope.ServiceProvider;
            var repo = services.GetService<IRepository<Product>>();

            foreach (var orderLine in obj.OrderLine)
            {
                var p = await repo.Get(orderLine.ProductId);
                p.ItemsReserved -= orderLine.Quantity;
                await repo.Edit(p);
            }
        }

        private async void HandleOrderCancelled(OrderStatusChangedMessage obj)
        { 
            using var scope = provider.CreateScope();
            var services = scope.ServiceProvider;
            var repo = services.GetService<IRepository<Product>>();

            foreach (var orderLine in obj.OrderLine)
            {
                var product = await repo.Get(orderLine.ProductId);
                product.ItemsReserved -= orderLine.Quantity;
                product.ItemsInStock += orderLine.Quantity;
                await repo.Edit(product);
            }
        }
        
        private async void HandleProductCheck(OrderCreatedMessage message)
        {
            Console.WriteLine("ProductListener HandleProductCheck");
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
                    Console.WriteLine(orderLine.Quantity);
                    var product =await productRepos.Get(orderLine.ProductId);
                    product.ItemsReserved += orderLine.Quantity;
                    product.ItemsInStock -= orderLine.Quantity;
                    await productRepos.Edit(product);
                }

                var replyMessage = new OrderAcceptedMessage
                {
                    OrderId = message.OrderId,
                    CustomerId = message.CustomerId,
                };
                    
                await _bus.PubSub.PublishAsync(replyMessage);
                Console.WriteLine("ProductListener PublishOrderAcceptedMessage");
            }
            else
            {
                // publish an OrderRejectedMessage
                var replyMessage = new OrderRejectedMessage()
                {
                    OrderId = message.OrderId
                };
                await _bus.PubSub.PublishAsync(replyMessage);
                Console.WriteLine("ProductListener PublishOrderRejectedMessage");
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
                    Console.WriteLine("ProductListener Not enough products");
                    return false;
                }
            }
            return true;
        }

    }
}