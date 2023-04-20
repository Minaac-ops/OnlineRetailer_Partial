using System;
using System.Collections.Generic;
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

        public async void PublishOrderCreatedMessage(int? customerId, int orderId, IList<OrderLine> orderLines)
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

        public async void CreditStandingChangedMessage(int orderResultCustomerId)
        {
            var message = new CreditStandingChangedMessage
            {
                CustomerId = orderResultCustomerId
            };
            await bus.PubSub.PublishAsync(message, "paid");
        }

        public async void OrderStatusChangedMessage(int id,IList<OrderLine> orderLines, string topic)
        {
            var message = new OrderStatusChangedMessage
            {
                OrderId = id,
                OrderLine = orderLines
            };
            await bus.PubSub.PublishAsync(message, $"{topic}");
        }

        public async void PublishOrderAccepted(int orderCustomerId, int orderId)
        {
            var message = new EmailMessage
            {
                CustomerId = orderCustomerId,
                OrderId = orderId
            };

            await bus.PubSub.PublishAsync(message,"OrderConfirmed");

        }

        public async void PublishOrderCancelled(int orderCustomerId, int orderId)
        {
            var message = new EmailMessage
            {
                CustomerId = orderCustomerId,
                OrderId = orderId
            };

            await bus.PubSub.PublishAsync(message, "Cancelled");
        }

        public async void PublishOrderShippedEmail(int customerId, int orderId)
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