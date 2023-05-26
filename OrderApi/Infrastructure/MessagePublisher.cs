using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using Dapr.Client;
using EasyNetQ;
using Monitoring;
using OpenTelemetry;
using OpenTelemetry.Context.Propagation;
using Shared;

namespace OrderApi.Infrastructure
{
    public class MessagePublisher : IMessagePublisher, IDisposable
    {
        IBus bus;

        public void Dispose()
        {
            bus.Dispose();
        }

        public async Task PublishOrderCreatedMessage(int? customerId, int orderId, IList<OrderLine> orderLines)
        {
            using var activity = MonitorService.ActivitySource.StartActivity();
            using var daprClient = new DaprClientBuilder().Build();
            MonitorService.Log.Here().Debug("Entered PublishOrderCreatedMessage to publish OrderCreatedMessage");
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
            
            //await bus.PubSub.PublishAsync(messageCustomer, "checkCredit");
            await daprClient.PublishEventAsync("orderpubsub", "checkCredit", messageCustomer);
            MonitorService.Log.Here().Debug("Published DaprOrderCreatedMessage to CustomerApi");
            Console.WriteLine("Published DaprData: "+ messageCustomer.CustomerId);
            
            //await bus.PubSub.PublishAsync(messageProduct, "checkProductAvailability");
            await daprClient.PublishEventAsync("orderpubsub", "checkProductAvailability", messageProduct);
            MonitorService.Log.Here().Debug("Published DaprOrderCreatedMessage to ProductApi");
            Console.WriteLine("Published Daprdata: " + messageProduct.OrderLines);
        }

        public async Task CreditStandingChangedMessage(int customerId)
        {
            using var daprClient = new DaprClientBuilder().Build();
            var message = new CreditStandingChangedMessage
            {
                CustomerId = customerId
            };

            await daprClient.PublishEventAsync<CreditStandingChangedMessage>("orderpubsub", "creditChange", message);

            Console.WriteLine("CreditStatusChangeMessage: PublishedDapr customerId: " + message.CustomerId);
        }

        public async Task OrderStatusChangedMessage(int id,IList<OrderLine> orderLines, string topic)
        {
            using var activity = MonitorService.ActivitySource.StartActivity();
            MonitorService.Log.Here().Debug("OrderStatusChangedMessage before publish");
            var message = new OrderStatusChangedMessage
            {
                OrderId = id,
                OrderLine = orderLines
            };
            
            await bus.PubSub.PublishAsync(message, $"{topic}");
            MonitorService.Log.Here().Debug("OrderStatusChangedMessage after publish");
        }

        public async Task PublishOrderAccepted(int orderCustomerId, int orderId)
        {
            using var activity = MonitorService.ActivitySource.StartActivity();
            using var daprClient = new DaprClientBuilder().Build();
            MonitorService.Log.Here().Debug("PublishOrderAccepted before publish");
            var message = new EmailMessage
            {
                CustomerId = orderCustomerId,
                OrderId = orderId
            };
            
            // Adding header to the message so the activity can continue in emailService
            var activityCtx = activity?.Context ?? Activity.Current?.Context ?? default;
            var propagationCtx = new PropagationContext(activityCtx, Baggage.Current);
            var propagator = new TraceContextPropagator();
            propagator.Inject(propagationCtx, message, (r, key, value) =>
            {
                r.Header.Add(key, value);
            });

            await bus.PubSub.PublishAsync(message,"OrderConfirmed");
            //await daprClient.PublishEventAsync("orderpubsub", "OrderConfirmed", message);
            
            MonitorService.Log.Here().Debug("PublishOrderAccepted after publish");
        }

        public async Task PublishOrderCancelled(int orderCustomerId, int orderId)
        {
            using var activity = MonitorService.ActivitySource.StartActivity();
            MonitorService.Log.Here().Debug("PublishOrderCancelled before publish");
            
            var message = new EmailMessage
            {
                CustomerId = orderCustomerId,
                OrderId = orderId
            };
            
            // Adding header to the message so the activity can continue in emailService
            var activityCtx = activity?.Context ?? Activity.Current?.Context ?? default;
            var propagationCtx = new PropagationContext(activityCtx, Baggage.Current);
            var propagator = new TraceContextPropagator();
            propagator.Inject(propagationCtx, message, (r, key, value) =>
            {
                r.Header.Add(key, value);
            });

            await bus.PubSub.PublishAsync(message, "Cancelled");
            MonitorService.Log.Here().Debug("PublishOrderCancelled after publish");
        }

        public async Task PublishOrderShippedEmail(int customerId, int orderId)
        {
            using var activity = MonitorService.ActivitySource.StartActivity();
            MonitorService.Log.Here().Debug("PublishOrderShippedEmail before handle");
            var message = new EmailMessage
            {
                CustomerId = customerId,
                OrderId = orderId
            };
            
            // Adding header to the message so the activity can continue in emailService
            var activityCtx = activity?.Context ?? Activity.Current?.Context ?? default;
            var propagationCtx = new PropagationContext(activityCtx, Baggage.Current);
            var propagator = new TraceContextPropagator();
            propagator.Inject(propagationCtx, message, (r, key, value) =>
            {
                r.Header.Add(key, value);
            });
            
            await bus.PubSub.PublishAsync(message, "Shipped");
            MonitorService.Log.Here().Debug("PublishOrderShippedEmail after  handle");
        }
    }
}