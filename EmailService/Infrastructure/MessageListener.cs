using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using EasyNetQ;
using EmailService.Models;
using Microsoft.Extensions.DependencyInjection;
using Monitoring;
using OpenTelemetry;
using OpenTelemetry.Context.Propagation;
using RestSharp;
using Shared;

namespace EmailService.Infrastructure
{
    public class MessageListener
    {
        private IServiceProvider _provider;
        private string _connectionString;
        private IBus _bus;

        public MessageListener(IServiceProvider provider,
            string connectionString)
        {
            _provider = provider;
            _connectionString = connectionString;
        }

        public async Task Start()
        {
            using (_bus = RabbitHutch.CreateBus(_connectionString))
            {
                await _bus.PubSub.SubscribeAsync<EmailMessage>(
                    "sendConfirmationEmail", HandleConfirmationEmail, x => x.WithTopic("OrderConfirmed"));

                MonitorService.Log.Here().Debug("EmailListener: Subscribing to OrderAccepted");

                await _bus.PubSub.SubscribeAsync<EmailMessage>(
                    "sendCancellationEmail", HandleCancellationEmail, x => x.WithTopic("Cancelled"));
                MonitorService.Log.Here().Debug("EmailListener: Subscribing to OrderCancelled");

                await _bus.PubSub.SubscribeAsync<EmailMessage>(
                    "sendOrderShippedEmail", HandleSendShippedEmail, x => x.WithTopic("Shipped"));
                MonitorService.Log.Here().Debug("EmailListener: subscribing to OrderShipped");

                lock (this)
                {
                    Monitor.Wait(this);
                }
            }
        }

        private async void HandleSendShippedEmail(EmailMessage arg)
        {
            MonitorService.Log.Here().Debug("HandleSendShippedEmail before handle");
            
            // Propagator to continue the activity from OrderController
            var propagator = new TraceContextPropagator();
            var parentCtx = propagator.Extract(default, arg,
                (r, key) =>
                {
                    return new List<string>(new[]
                        {r.Header.ContainsKey(key) ? r.Header[key].ToString() : string.Empty});
                });
            Baggage.Current = parentCtx.Baggage;
            using var activity = MonitorService.ActivitySource.StartActivity("Message received", ActivityKind.Consumer,
                parentCtx.ActivityContext);
            
            using var scope = _provider.CreateScope();
            var service = scope.ServiceProvider;
            var repo = service.GetService<IEmailSender>();
            var customer = await GetCustomer(arg.CustomerId);
            
            MonitorService.Log.Here().Debug("Email to send confirmation email to: " + customer.Email);

            var message = new Message(new string[] {customer.Email}, customer.CompanyName,
                "Order shipped!", "Your order has been shipped! It will be delivered in 2-3 business days.");
            await repo.SendEmail(message);
            MonitorService.Log.Here().Debug("HandleSendShippedEmail after handle");
        }

        private async void HandleCancellationEmail(EmailMessage obj)
        {
            MonitorService.Log.Here().Debug("HandleCancellationEmail before handle");
            
            // Propagator to continue the activity from OrderController
            var propagator = new TraceContextPropagator();
            var parentCtx = propagator.Extract(default, obj,
                (r, key) =>
                {
                    return new List<string>(new[]
                        {r.Header.ContainsKey(key) ? r.Header[key].ToString() : string.Empty});
                });
            Baggage.Current = parentCtx.Baggage;
            using var activity = MonitorService.ActivitySource.StartActivity("Message received", ActivityKind.Consumer,
                parentCtx.ActivityContext);

            using var scope = _provider.CreateScope();
            var service = scope.ServiceProvider;
            var repo = service.GetService<IEmailSender>();
            var customer = await GetCustomer(obj.CustomerId);
            
            MonitorService.Log.Here().Debug("Email to send confirmation email to: " + customer.Email);

            var message = new Message(new string[] {customer.Email}, customer.CompanyName,
                "Order cancelled!",
                "Your order was not cancelled either because:\n" +
                "- We don't have the requested products\n" +
                "- Your account has bad credit\n" +
                "- You cancelled your order");
            await repo.SendEmail(message);

            MonitorService.Log.Here().Debug("HandleCancellationEmail after handle");
        }

        private async void HandleConfirmationEmail(EmailMessage arg)
        {
            MonitorService.Log.Here().Debug("HandleConfirmationEmail before handle"); 
            
            // Propagator to continue the activity from OrderController
            var propagator = new TraceContextPropagator();
            var parentCtx = propagator.Extract(default, arg,
                (r, key) =>
                {
                    return new List<string>(new[]
                        {r.Header.ContainsKey(key) ? r.Header[key].ToString() : string.Empty});
                });
            Baggage.Current = parentCtx.Baggage;
            using var activity = MonitorService.ActivitySource.StartActivity("Message received", ActivityKind.Consumer,
                parentCtx.ActivityContext);

            using var scope = _provider.CreateScope();
            var service = scope.ServiceProvider;
            var repo = service.GetService<IEmailSender>();
            var customer = await GetCustomer(arg.CustomerId);

            MonitorService.Log.Here().Debug("Email to send confirmation email to: " + customer.Email);

            var message = new Message(new string[] {customer.Email}, customer.CompanyName, "Order accepted",
                "Congratulations! Your order was accepted!");
            await repo.SendEmail(message);
            MonitorService.Log.Here().Debug("HandleConfirmationEmail handled");
        }

        private async Task<CustomerDto> GetCustomer(int customerId)
        {
            using var activity = MonitorService.ActivitySource.StartActivity();
            RestClient client = new RestClient("http://customerapi/customer/");
            var request = new RestRequest(customerId.ToString());

            var customerResponse = await client.GetAsync<CustomerDto>(request);
            return customerResponse ?? throw new InvalidOperationException();
        }
    }
}