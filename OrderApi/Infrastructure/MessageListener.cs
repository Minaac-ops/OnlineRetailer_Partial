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
                bus.PubSub.Subscribe<OrderAcceptedMessage>("orderAcceptedMessage", HandleOrderAccepted);
                Console.WriteLine("Subscribing to OrderAcceptedMessage");

                bus.PubSub.Subscribe<OrderRejectedMessage>("orderRejectedMessage", HandleOrderRejected);
                Console.WriteLine("Subscribing to OrderRejectedMessage");
                
                //block thread so it doesnt stop sub
                lock (this)
                {
                    Monitor.Wait(this);
                }
            }
        }

        private void HandleOrderRejected(OrderRejectedMessage obj)
        {
            Console.WriteLine("Received OrderRejectedMessage");
            using var scope = _provider.CreateScope();
            var services = scope.ServiceProvider;
            var orderRepo = services.GetService<IRepository<Order>>();
            
            //Delete order
            var order = orderRepo.Get(obj.OrderId);

            order.Status = OrderStatus.Cancelled;
            orderRepo.Edit(order.Id,order);
            Console.WriteLine("Handled OrderRejectedMessage");
        }

        private void HandleOrderAccepted(OrderAcceptedMessage obj)
        {
            Console.WriteLine("ReceivedOrderAccepted");
            using var scope = _provider.CreateScope();
            var services = scope.ServiceProvider;
            var orderRepo = services.GetService<IRepository<Order>>();
            
            //Mark order as completed
            var order = orderRepo.Get(obj.OrderId);
            order.Status = OrderStatus.Completed;
            orderRepo.Edit(order.Id, order);
            Console.WriteLine("HandleOrderAccepted");
        }
    }
}