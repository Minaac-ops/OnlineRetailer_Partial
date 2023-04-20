using System;
using System.Threading.Tasks;
using EmailService.Models;
using FeatureHubSDK;
using MailKit.Net.Smtp;
using MimeKit;
using Org.BouncyCastle.Bcpg.OpenPgp;

namespace EmailService.Infrastructure
{
    public class EmailSender : IEmailSender
    {
        private readonly EmailConfig _emailConfig;
        private readonly EdgeFeatureHubConfig _featureHubConfig;
        
        public EmailSender(EmailConfig emailConfig)
        {
            _emailConfig = emailConfig;
            _featureHubConfig = new EdgeFeatureHubConfig("http://localhost:8085","a54098bc-b44b-4b07-95ae-ba5df4d147d8/s4M0YTSp3vHP56yoFsFJdI1u9vp1cvDwONfdBZAa");
        }

        public async Task SendEmail(Message message)
        {
            var fh = await _featureHubConfig.NewContext().Build();
            if (fh["SendEmailFeature"].IsEnabled)
            {
                var emailMessage = await CreateEmailMessage(message);
                Console.WriteLine(message.To);
                await Send(emailMessage);
            }
            else
            {
                throw new NotImplementedException();
            }
        }

        private async Task<MimeMessage> CreateEmailMessage(Message message)
        {
            var emailMessage = new MimeMessage();
            emailMessage.From.Add(new MailboxAddress(message.CustomerName,_emailConfig.From));
            emailMessage.To.AddRange(message.To);
            emailMessage.Subject = message.Subject;
            emailMessage.Body = new TextPart(MimeKit.Text.TextFormat.Text) {Text = message.Content};
            return emailMessage;
        }

        private async Task Send(MimeMessage mailMessage)
        {
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