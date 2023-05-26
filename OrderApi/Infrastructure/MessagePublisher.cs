using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using Dapr.Client;
using Monitoring;
using OpenTelemetry;
using OpenTelemetry.Context.Propagation;
using Shared;

namespace OrderApi.Infrastructure
{
    public class MessagePublisher : IMessagePublisher
    {
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
            
            await daprClient.PublishEventAsync("orderpubsub", "checkCredit", messageCustomer);
            MonitorService.Log.Here().Debug("Published DaprOrderCreatedMessage to CustomerApi");
            Console.WriteLine("ORDER PUBLISHER PUBLISHED DAPRMESSAGE. customerid "+ messageCustomer.CustomerId+". orderid: " + messageCustomer.OrderId);
            
            await daprClient.PublishEventAsync("orderpubsub", "checkProductAvailability", messageProduct);
            MonitorService.Log.Here().Debug("Published DaprOrderCreatedMessage to ProductApi");
            Console.WriteLine("ORDER PUBLISHER PUBLISHED DAPRMESSAGE. Orderid: " + messageProduct.OrderId + "products: ");
            foreach (var VARIABLE in messageProduct.OrderLines)
            {
                Console.WriteLine(VARIABLE.ProductId);
            }
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
            using var daprClient = new DaprClientBuilder().Build();
            using var activity = MonitorService.ActivitySource.StartActivity();
            
            MonitorService.Log.Here().Debug("OrderStatusChangedMessage before publish");
            var message = new OrderStatusChangedMessage
            {
                OrderId = id,
                OrderLine = orderLines
            };
            
            await daprClient.PublishEventAsync<OrderStatusChangedMessage>("orderpubsub", $"{topic}", message);
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

            await daprClient.PublishEventAsync("orderpubsub", "OrderConfirmed", message);
            Console.WriteLine("ORDERPUBLISHER PUBLISHING ORDER CONFIRMED IN RELATIONS TO ORDERCREATED FINISHED PROCESSING. " + message.CustomerId);
            
            MonitorService.Log.Here().Debug("PublishOrderAccepted after publish");
        }

        public async Task PublishOrderCancelled(int orderCustomerId, int orderId)
        {
            using var daprClient = new DaprClientBuilder().Build();
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

            await daprClient.PublishEventAsync("orderpubsub", "Cancelled", message);
            MonitorService.Log.Here().Debug("PublishOrderCancelled after publish");
        }

        public async Task PublishOrderShippedEmail(int customerId, int orderId)
        {
            using var daprClient = new DaprClientBuilder().Build();
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
            
            await daprClient.PublishEventAsync("orderpubsub", "Shipped", message);
            MonitorService.Log.Here().Debug("PublishOrderShippedEmail after  handle");
        }
    }
}