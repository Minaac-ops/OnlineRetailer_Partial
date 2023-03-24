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

        public void PublishOrderCreatedMessage(int? customerId, int orderId, IList<OrderLine> orderLines)
        {
            Console.WriteLine("hej publisher");
            OrderCreatedMessage message = new OrderCreatedMessage
            { 
                CustomerId = customerId,
                OrderId = orderId,
                OrderLines = orderLines 
            };
            bus.PubSub.Publish(message);
        }

        public void CreditStandingChangedMessage(int orderResultCustomerId)
        {
            var message = new CreditStandingChangedMessage
            {
                CustomerId = orderResultCustomerId
            };
            bus.PubSub.Publish(message);
        }

        public void OrderStatusChangedMessage(int id,IList<OrderLine> orderLines, string topic)
        {
            var message = new OrderStatusChangedMessage
            {
                OrderId = id,
                OrderLine = orderLines,
                Topic = topic,
            };
            bus.PubSub.Publish(message);
        }
    }
}