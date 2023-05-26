using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using Dapr;
using EmailService.Infrastructure;
using EmailService.Models;
using Microsoft.AspNetCore.Mvc;
using Monitoring;
using OpenTelemetry;
using OpenTelemetry.Context.Propagation;
using RestSharp;
using Shared;

namespace EmailService.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class MessageListenerController : ControllerBase
    {
        private readonly IEmailSender _emailSender;

        public MessageListenerController(IEmailSender emailSender)
        {
            _emailSender = emailSender;
        }

        [Topic("orderpubsub", "OrderConfirmed")]
        [HttpPost("/confirmation")]
        public async Task HandleConfirmationEmail([FromBody] EmailMessage msg)
        {
            MonitorService.Log.Here().Debug("HandleConfirmationEmail before handle"); 
            
            // Propagator to continue the activity from OrderController
            var propagator = new TraceContextPropagator();
            var parentCtx = propagator.Extract(default, msg,
                (r, key) =>
                {
                    return new List<string>(new[]
                        {r.Header.ContainsKey(key) ? r.Header[key].ToString() : string.Empty});
                });
            Baggage.Current = parentCtx.Baggage;
            using var activity = MonitorService.ActivitySource.StartActivity("Message received", ActivityKind.Consumer,
                parentCtx.ActivityContext);

            
            var customer = await GetCustomer(msg.CustomerId);

            MonitorService.Log.Here().Debug("Email to send confirmation email to: " + customer.Email);

            var message = new Message(new string[] {customer.Email}, customer.CompanyName, "Order accepted",
                "Congratulations! Your order was accepted!");
            await _emailSender.SendEmail(message);
            MonitorService.Log.Here().Debug("HandleConfirmationEmail handled");
        }

        [Topic("orderpubsub", "Cancelled")]
        [HttpPost("/cancellation")]
        public async Task HandleCancellationEmail([FromBody] EmailMessage msg)
        {
            MonitorService.Log.Here().Debug("HandleCancellationEmail before handle");
            
            // Propagator to continue the activity from OrderController
            var propagator = new TraceContextPropagator();
            var parentCtx = propagator.Extract(default, msg,
                (r, key) =>
                {
                    return new List<string>(new[]
                        {r.Header.ContainsKey(key) ? r.Header[key].ToString() : string.Empty});
                });
            Baggage.Current = parentCtx.Baggage;
            using var activity = MonitorService.ActivitySource.StartActivity("Message received", ActivityKind.Consumer,
                parentCtx.ActivityContext);

            var customer = await GetCustomer(msg.CustomerId);
            
            MonitorService.Log.Here().Debug("Email to send confirmation email to: " + customer.Email);

            var message = new Message(new string[] {customer.Email}, customer.CompanyName,
                "Order cancelled!",
                "Your order was not cancelled either because:\n" +
                "- We don't have the requested products\n" +
                "- Your account has bad credit\n" +
                "- You cancelled your order");
            await _emailSender.SendEmail(message);

            MonitorService.Log.Here().Debug("HandleCancellationEmail after handle");
        }

        [Topic("orderpubsub", "Shipped")]
        [HttpPost("/shipped")]
        public async Task HandleSendShippedEmail([FromBody] EmailMessage msg)
        {
            MonitorService.Log.Here().Debug("HandleSendShippedEmail before handle");
            
            // Propagator to continue the activity from OrderController
            var propagator = new TraceContextPropagator();
            var parentCtx = propagator.Extract(default, msg,
                (r, key) =>
                {
                    return new List<string>(new[]
                        {r.Header.ContainsKey(key) ? r.Header[key].ToString() : string.Empty});
                });
            Baggage.Current = parentCtx.Baggage;
            using var activity = MonitorService.ActivitySource.StartActivity("Message received", ActivityKind.Consumer,
                parentCtx.ActivityContext);
            
            var customer = await GetCustomer(msg.CustomerId);
            
            MonitorService.Log.Here().Debug("Email to send confirmation email to: " + customer.Email);

            var message = new Message(new string[] {customer.Email}, customer.CompanyName,
                "Order shipped!", "Your order has been shipped! It will be delivered in 2-3 business days.");
            await _emailSender.SendEmail(message);
            MonitorService.Log.Here().Debug("HandleSendShippedEmail after handle");
        }
        
        private async Task<CustomerDto> GetCustomer(int customerId)
        {
            using var activity = MonitorService.ActivitySource.StartActivity();
            RestClient client = new RestClient("http://customerapi/customer/");
            var request = new RestRequest(customerId.ToString());

            var customerResponse = await client.GetAsync<CustomerDto>(request);
            Console.WriteLine("EmailController getting customer to send email, found companyname: " + customerResponse.CompanyName
            + " email: " + customerResponse.Email);
            return customerResponse ?? throw new InvalidOperationException();
        }
    }
}
