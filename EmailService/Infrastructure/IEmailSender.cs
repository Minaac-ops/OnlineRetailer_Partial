using System.Threading.Tasks;
using EmailService.Models;

namespace EmailService.Infrastructure
{
    public interface IEmailSender
    {
        Task SendEmail(Message message);
        
        Task SendNewsletter(Message message);
    }
}