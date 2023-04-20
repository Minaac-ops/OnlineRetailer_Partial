using System;
using System.Threading;
using System.Threading.Tasks;
using EasyNetQ;
using EmailService.Models;
using Microsoft.Extensions.DependencyInjection;
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

                Console.WriteLine("EmailListener: Subscribing to OrderAccepted");

                await _bus.PubSub.SubscribeAsync<EmailMessage>(
                    "sendCancellationEmail", HandleCancellationEmail, x => x.WithTopic("Cancelled"));
                Console.WriteLine("EmailListener: subscribing to OrderCancelled");

                await _bus.PubSub.SubscribeAsync<EmailMessage>(
                    "sendOrderShippedEmail", HandleSendShippedEmail, x => x.WithTopic("Shipped"));
                Console.WriteLine("EmailListener: subscribing to OrderShipped");
                
                lock (this)
                {
                    Monitor.Wait(this);
                }
            }
        }

        private async void HandleSendShippedEmail(EmailMessage arg)
        {
            Console.WriteLine("EmailListener handle sendshippedemail");
            using var scope = _provider.CreateScope();
            var service = scope.ServiceProvider;
            var repo = service.GetService<IEmailSender>();

            var customer = await GetCustomer(arg.CustomerId);
            var message = new Message(new string[] {customer.Email}, customer.CompanyName,
                "Order shipped!", "Your order has been shipped! It will be delivered in 2-3 business days.");
            await repo.SendEmail(message);
            Console.WriteLine("EmailListener: Handled ship email");
        }

        private async void HandleCancellationEmail(EmailMessage obj)
        {
            Console.WriteLine("Handle CancellationEmail");
            using var scope = _provider.CreateScope();
            var service = scope.ServiceProvider;
            var repo = service.GetService<IEmailSender>();

            var customer = await GetCustomer(obj.CustomerId);

            if (customer == null)
            {
                Console.WriteLine("Couldnt get customer");
            }
            else
            {
                var message = new Message(new string[] {customer.Email}, customer.CompanyName,
                    "Order cancelled!",
                    "Your order was not cancelled either because:\n" +
                    "- We don't have the requested products\n" +
                    "- Your account has bad credit\n" +
                    "- You cancelled your order");
                await repo.SendEmail(message);
            }
            Console.WriteLine("Email Listener: HandledCancelation ");
        }

        private async void HandleConfirmationEmail(EmailMessage arg)
        {
            Console.WriteLine("handle confirmationemail");
            using var scope = _provider.CreateScope();
            var service = scope.ServiceProvider;
            var repo = service.GetService<IEmailSender>();

            var customer = await GetCustomer(arg.CustomerId);

            if (customer == null)
            {
                Console.WriteLine("couldnt get customer");
            }
            else
            {
                Console.WriteLine("EmailListener customer email: " + customer.Email);
                var message = new Message(new string[] {customer.Email}, customer.CompanyName, "Order accepted",
                    "Congratulations! Your order was accepted!");
                await repo.SendEmail(message);
                Console.WriteLine("handled confirmationemail");
            }
        }

        private async Task<CustomerDto> GetCustomer(int customerId)
        {
            RestClient client = new RestClient("http://customerapi/customer/");
            
            var request = new RestRequest(customerId.ToString());
            var customerResponse = await client.GetAsync<CustomerDto>(request);
            return customerResponse ?? throw new InvalidOperationException();
        }
    }
}