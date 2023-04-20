using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using EasyNetQ;
using Shared;

namespace OrderApi.Infrastructure
{
    public class MessagePublisher : IMessagePublisher, IDisposable
    {
        IBus bus;

        public MessagePublisher(string connectionString)
        {
            bus = RabbitHutch.CreateBus(connectionString);
        }

        public void Dispose()
        {
            bus.Dispose();
        }

        public async Task PublishOrderCreatedMessage(int? customerId, int orderId, IList<OrderLine> orderLines)
        {
            Console.WriteLine("publisheorderCreateMessage topic: ");
            var messageCustomer = new OrderCreatedMessage
            { 
                CustomerId = customerId,
                OrderId = orderId,
                OrderLines = orderLines,
            };
            
            var messageProduct = new OrderCreatedMessage
            { 
                CustomerId = customerId,
                OrderId = orderId,
                OrderLines = orderLines,
            };
            
            await bus.PubSub.PublishAsync(messageCustomer, "checkCredit");
            Console.WriteLine("OrderPublisher OrderCreatedMessage to customer after publish.");
            await bus.PubSub.PublishAsync(messageProduct, "checkProductAvailability");
            Console.WriteLine("OrderPublisher OrderCreatedMessage to product after publish.");
        }

        public async Task CreditStandingChangedMessage(int orderResultCustomerId)
        {
            var message = new CreditStandingChangedMessage
            {
                CustomerId = orderResultCustomerId
            };
            await bus.PubSub.PublishAsync(message, "paid");
        }

        public async Task OrderStatusChangedMessage(int id,IList<OrderLine> orderLines, string topic)
        {
            var message = new OrderStatusChangedMessage
            {
                OrderId = id,
                OrderLine = orderLines
            };
            await bus.PubSub.PublishAsync(message, $"{topic}");
        }

        public async Task PublishOrderAccepted(int orderCustomerId, int orderId)
        {
            var message = new EmailMessage
            {
                CustomerId = orderCustomerId,
                OrderId = orderId
            };

            await bus.PubSub.PublishAsync(message,"OrderConfirmed");

        }

        public async Task PublishOrderCancelled(int orderCustomerId, int orderId)
        {
            var message = new EmailMessage
            {
                CustomerId = orderCustomerId,
                OrderId = orderId
            };

            await bus.PubSub.PublishAsync(message, "Cancelled");
        }

        public async Task PublishOrderShippedEmail(int customerId, int orderId)
        {
            var message = new EmailMessage
            {
                CustomerId = customerId,
                OrderId = orderId
            };
            await bus.PubSub.PublishAsync(message, "Shipped");
        }
    }
}