using System;
using System.Threading.Tasks;
using EmailService.Models;
using FeatureHubSDK;
using MailKit.Net.Smtp;
using Microsoft.FeatureManagement;
using MimeKit;
using Monitoring;
using Org.BouncyCastle.Bcpg.OpenPgp;

namespace EmailService.Infrastructure
{
    public class EmailSender : IEmailSender
    {
        private readonly EmailConfig _emailConfig;
        //316272c6-8209-4388-b46d-489edf263f3f/laAFIB2PpdWNHwPv1hmZE7VZNbJR5IzrTNOMfYFs Minas FeatureHub key
        
        public EmailSender(EmailConfig emailConfig)
        {
            _emailConfig = emailConfig;
        }

        public async Task SendEmail(Message message)
        {
            var config = new EdgeFeatureHubConfig("http://featurehub:8085", "316272c6-8209-4388-b46d-489edf263f3f/laAFIB2PpdWNHwPv1hmZE7VZNbJR5IzrTNOMfYFs");
            var fh = await config.NewContext().Build();
            if (fh["SendEmailFeature"].IsEnabled)
            {
                using var activity = MonitorService.ActivitySource.StartActivity();
                MonitorService.Log.Here().Debug("Entered SendEmail to prepare email for sending");
                var emailMessage = await CreateEmailMessage(message);
                Console.WriteLine(message.To);
                await Send(emailMessage);
            }
            else MonitorService.Log.Here().Debug("Didnt send email because Feature is disabled");
        }

        public async Task SendNewsletter(Message message)
        {
            var config = new EdgeFeatureHubConfig("http://featurehub:8085", "22bff5a8-bc8a-4264-be7b-ff074433adcb/jvXSfv9ikUTrwE66RuALT9OTXL1k0CUL3VEdzQtV");
            var fh = await config.NewContext().Build();
            if(fh["SendNewsletterFeature"].IsEnabled)
            {
                using var activity = MonitorService.ActivitySource.StartActivity();
                MonitorService.Log.Here().Debug("Entered SendNewsletter to prepare email for sending");
                var emailMessage = await CreateNewsletterMessage(message);
                await Send(emailMessage);
            }
            else MonitorService.Log.Here().Debug("Didnt send newsletter because Feature is disabled");
        }

        private async Task<MimeMessage> CreateEmailMessage(Message message)
        {
            using var activity = MonitorService.ActivitySource.StartActivity();
            MonitorService.Log.Here().Debug("Entered CreateEmailMessage to make the email");
            var emailMessage = new MimeMessage();
            emailMessage.From.Add(new MailboxAddress(message.CustomerName,_emailConfig.From));
            emailMessage.To.AddRange(message.To);
            emailMessage.Subject = message.Subject;
            emailMessage.Body = new TextPart(MimeKit.Text.TextFormat.Text) {Text = message.Content};
            return emailMessage;
        }
        
        private async Task<MimeMessage> CreateNewsletterMessage(Message message)
        {
            using var activity = MonitorService.ActivitySource.StartActivity();
            MonitorService.Log.Here().Debug("Entered CreateNewsletterMessage to make the email");
            var emailMessage = new MimeMessage();
            emailMessage.From.Add(new MailboxAddress("Newsletter", "Hovedkvartergade 1"));
            emailMessage.To.AddRange(message.To);
            emailMessage.Subject = message.Subject;
            emailMessage.Body = new TextPart(MimeKit.Text.TextFormat.Text) {Text = message.Content};
            return emailMessage;
        }

        private async Task Send(MimeMessage mailMessage)
        {
            using var activity = MonitorService.ActivitySource.StartActivity();
            MonitorService.Log.Here().Debug("Entered Send to send email");
            using (var client = new SmtpClient())
            {
                try
                {
                    await client.ConnectAsync(_emailConfig.SmtpServer, _emailConfig.Port, true);
                    client.AuthenticationMechanisms.Remove("XOAUTH2");
                    await client.AuthenticateAsync(_emailConfig.UserName, _emailConfig.Password);
                    await client.SendAsync(mailMessage);
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                    throw;
                }
                finally
                {
                    await client.DisconnectAsync(true);
                    client.Dispose();
                }
            }
        }
    }
}