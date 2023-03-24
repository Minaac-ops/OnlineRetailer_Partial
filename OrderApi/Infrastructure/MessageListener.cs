using System;
using System.Threading;
using EasyNetQ;
using Microsoft.Extensions.DependencyInjection;
using OrderApi.Data;
using OrderApi.Models;
using Shared;

namespace OrderApi.Infrastructure
{
    public class MessageListener
    {
        private IServiceProvider _provider;
        private string connectionString;
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
                bus.PubSub.Subscribe<OrderAcceptedMessage>("orderAcceptedMessage", HandleOrderAccepted);

                bus.PubSub.Subscribe<OrderRejectedMessage>("orderRejectedMessage", HandleOrderRejected);
                
                //block thread so it doesnt stop sub
                lock (this)
                {
                    Monitor.Wait(this);
                }
            }
        }

        private void HandleOrderRejected(OrderRejectedMessage obj)
        {
            using var scope = _provider.CreateScope();
            var services = scope.ServiceProvider;
            var orderRepo = services.GetService<IRepository<Order>>();
            
            //Delete order
            orderRepo.Remove(obj.OrderId);
        }

        private void HandleOrderAccepted(OrderAcceptedMessage obj)
        {
            using var scope = _provider.CreateScope();
            var services = scope.ServiceProvider;
            var orderRepo = services.GetService<IRepository<Order>>();
            
            //Mark order as completed
            var order = orderRepo.Get(obj.OrderId);
            order.Status = OrderStatus.Completed;
            orderRepo.Edit(order.Id, order);
        }
    }
}