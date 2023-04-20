using System;
using System.Collections.Generic;
using System.Linq;
using MimeKit;

namespace EmailService.Models
{
    public class Message
    {
        public List<MailboxAddress> To { get; set; }
        public string CustomerName { get; set; }
        public string Subject { get; set; }
        public string Content { get; set; }

        public Message(IEnumerable<string> to,string customerName, string subject, string content)
        {
            To = new List<MailboxAddress>();
            CustomerName = customerName;
            To.AddRange(to.Select(x => new MailboxAddress(customerName,x)));
            Subject = subject;
            Content = content;
        }
    }
}