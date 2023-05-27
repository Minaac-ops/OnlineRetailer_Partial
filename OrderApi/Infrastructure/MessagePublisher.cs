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
            using var daprClient = new DaprClientBuilder().Build();
            MonitorService.Log.Here().Debug("OrderApi: MessagePublisher PublishOrderCreatedMessage");

            var message = new OrderCreatedMessage
            { 
                CustomerId = customerId,
                OrderId = orderId,
                OrderLines = orderLines,
            };
            
            // Adding header to the message so the activity can continue in emailService
            var activityCtx = Activity.Current?.Context ?? default;
            var propagationCtx = new PropagationContext(activityCtx, Baggage.Current);
            var propagator = new TraceContextPropagator();
            propagator.Inject(propagationCtx, message, (r, key, value) =>
            {
                r.Header.Add(key, value);
            });
            
            await daprClient.PublishEventAsync("orderpubsub", "checkCredit", message);
            MonitorService.Log.Here().Debug("OrderApi: MessagePublisher published OrderCreatedMessage to CustomerApi");
            
            await daprClient.PublishEventAsync("orderpubsub", "checkProductAvailability", message);
            MonitorService.Log.Here().Debug("OrderApi: MessagePublisher published OrderCreatedMessage to ProductApi");
        }

        public async Task CreditStandingChangedMessage(int customerId)
        {
            MonitorService.Log.Here().Debug("OrderApi: MessagePublisher CreditStandingChangedMessage");
            using var daprClient = new DaprClientBuilder().Build();
            var message = new CreditStandingChangedMessage
            {
                CustomerId = customerId
            };
            
            // Adding header to the message so the activity can continue in emailService
            var activityCtx = Activity.Current?.Context ?? default;
            var propagationCtx = new PropagationContext(activityCtx, Baggage.Current);
            var propagator = new TraceContextPropagator();
            propagator.Inject(propagationCtx, message, (r, key, value) =>
            {
                r.Header.Add(key, value);
            });

            await daprClient.PublishEventAsync<CreditStandingChangedMessage>("orderpubsub", "creditChange", message);
            MonitorService.Log.Here().Debug("OrderApi: MessagePublisher published CreditStandingChangedMessage");
        }

        public async Task OrderStatusChangedMessage(int id,IList<OrderLine> orderLines, string topic)
        {
            using var daprClient = new DaprClientBuilder().Build();
            
            MonitorService.Log.Here().Debug("OrderApi: MessagePublisher OrderStatusChangedMessage");
            var message = new OrderStatusChangedMessage
            {
                OrderId = id,
                OrderLine = orderLines
            };
            
            // Adding header to the message so the activity can continue in emailService
            var activityCtx = Activity.Current?.Context ?? default;
            var propagationCtx = new PropagationContext(activityCtx, Baggage.Current);
            var propagator = new TraceContextPropagator();
            propagator.Inject(propagationCtx, message, (r, key, value) =>
            {
                r.Header.Add(key, value);
            });
            
            await daprClient.PublishEventAsync<OrderStatusChangedMessage>("orderpubsub", $"{topic}", message);
            MonitorService.Log.Here().Debug("OrderApi: MessagePublisher Published OrderStatusChangedMessage");
        }

        public async Task PublishOrderAccepted(int orderCustomerId, int orderId)
        {
            using var daprClient = new DaprClientBuilder().Build();
            MonitorService.Log.Here().Debug("OrderApi: MessagePublisher PublishOrderAccepted");
            var message = new EmailMessage
            {
                CustomerId = orderCustomerId,
                OrderId = orderId
            };
            
            // Adding header to the message so the activity can continue in emailService
            var activityCtx = Activity.Current?.Context ?? default;
            var propagationCtx = new PropagationContext(activityCtx, Baggage.Current);
            var propagator = new TraceContextPropagator();
            propagator.Inject(propagationCtx, message, (r, key, value) =>
            {
                r.Header.Add(key, value);
            });

            await daprClient.PublishEventAsync("orderpubsub", "OrderConfirmed", message);
            MonitorService.Log.Here().Debug("OrderApi: MessagePublisher published EmailMessage");
        }

        public async Task PublishOrderCancelled(int orderCustomerId, int orderId)
        {
            using var daprClient = new DaprClientBuilder().Build();
            MonitorService.Log.Here().Debug("OrderApi: MessagePublisher PublishOrderCancelled");
            
            var message = new EmailMessage
            {
                CustomerId = orderCustomerId,
                OrderId = orderId
            };
            
            // Adding header to the message so the activity can continue in emailService
            var activityCtx = Activity.Current?.Context ?? default;
            var propagationCtx = new PropagationContext(activityCtx, Baggage.Current);
            var propagator = new TraceContextPropagator();
            propagator.Inject(propagationCtx, message, (r, key, value) =>
            {
                r.Header.Add(key, value);
            });

            await daprClient.PublishEventAsync("orderpubsub", "Cancelled", message);
            MonitorService.Log.Here().Debug("OrderApi: MessagePublisher published EmailMessage");
        }

        public async Task PublishOrderShippedEmail(int customerId, int orderId)
        {
            using var daprClient = new DaprClientBuilder().Build();
            MonitorService.Log.Here().Debug("OrderApi: MessagePublisher PublishOrderShippedEmail");
            
            var message = new EmailMessage
            {
                CustomerId = customerId,
                OrderId = orderId
            };
            
            // Adding header to the message so the activity can continue in emailService
            var activityCtx = Activity.Current?.Context ?? default;
            var propagationCtx = new PropagationContext(activityCtx, Baggage.Current);
            var propagator = new TraceContextPropagator();
            propagator.Inject(propagationCtx, message, (r, key, value) =>
            {
                r.Header.Add(key, value);
            });

            await daprClient.PublishEventAsync("orderpubsub", "Shipped", message);
            MonitorService.Log.Here().Debug("OrderApi: MessagePublisher published EmailMessage");
        }
    }
}