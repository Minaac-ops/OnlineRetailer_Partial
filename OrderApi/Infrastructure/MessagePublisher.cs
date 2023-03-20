using System;
using System.Collections;
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

        public async void PublishOrderStatusChangedMessage(int customerId, IList<OrderLine> orderlines, string topic)
        {
            var message = new OrderStatusChangedMessage()
            {
                CustomerId = customerId,
                OrderLines = orderlines
            };
            await bus.PubSub.PublishAsync(message, topic);

        }
    }
}