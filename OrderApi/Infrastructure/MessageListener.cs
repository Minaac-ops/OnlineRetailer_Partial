using System;
using System.Threading;
using System.Threading.Tasks;
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
        IBus _bus;

        public MessageListener(IServiceProvider provider, string connectionString)
        {
            _provider = provider;
            this.connectionString = connectionString;
        }

        public async Task Start()
        {
            using (_bus = RabbitHutch.CreateBus(connectionString))
            {
                await _bus.PubSub.SubscribeAsync<OrderAcceptedMessage>("orderAcceptedMessage", HandleOrderAccepted);
                Console.WriteLine("Subscribing to OrderAcceptedMessage");

                await _bus.PubSub.SubscribeAsync<OrderRejectedMessage>("orderRejectedMessage", HandleOrderRejected);
                Console.WriteLine("Subscribing to OrderRejectedMessage");
                
                //block thread so it doesnt stop sub
                lock (this)
                {
                    Monitor.Wait(this);
                }
            }
        }

        private async void HandleOrderRejected(OrderRejectedMessage obj)
        {
            Console.WriteLine("Received OrderRejectedMessage");
            using var scope = _provider.CreateScope();
            var services = scope.ServiceProvider;
            var orderRepo = services.GetService<IRepository<Order>>();
            
            //Delete order
            var order = await orderRepo.Get(obj.OrderId);

            order.Status = OrderDto.OrderStatus.Cancelled;
            await orderRepo.Edit(order);
            Console.WriteLine("Handled OrderRejectedMessage");
        }

        private async void HandleOrderAccepted(OrderAcceptedMessage obj)
        {
            Console.WriteLine("ReceivedOrderAccepted");
            using var scope = _provider.CreateScope();
            var services = scope.ServiceProvider;
            var orderRepo = services.GetService<IRepository<Order>>();
            
            //Mark order as completed
            var order = await orderRepo.Get(obj.OrderId);
            Console.WriteLine("handleOrderAccepted orderstatus before edit "+order.Status + "orderid "+obj.OrderId);
            order.Status = OrderDto.OrderStatus.Completed;
            Console.WriteLine("handleOrderAccepted orderstaus after edit "+ order.Status);
            await orderRepo.Edit(order);
            var newOrder = await orderRepo.Get(obj.OrderId);
            Console.WriteLine("after save: "+newOrder.Status);
            Console.WriteLine("HandleOrderAccepted");
        }
    }
}