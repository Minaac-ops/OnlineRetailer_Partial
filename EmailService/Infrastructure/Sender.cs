using System;
using System.Threading.Tasks;
using EmailService.Models;
using MailKit.Net.Smtp;
using MimeKit;

namespace EmailService.Infrastructure
{
    public class Sender : ISender
    {
        private readonly EmailConfig _emailConfig;

        public Sender(EmailConfig emailConfig)
        {
            _emailConfig = emailConfig;
        }
        
        public async Task SendEmail(Message message)
        {
            var emailMessage = await CreateEmailMessage(message);
            Console.WriteLine(message.To);
            await Send(emailMessage);
        }

        private async Task<MimeMessage> CreateEmailMessage(Message message)
        {
            var emailMessage = new MimeMessage();
            emailMessage.From.Add(new MailboxAddress("email",_emailConfig.From));
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