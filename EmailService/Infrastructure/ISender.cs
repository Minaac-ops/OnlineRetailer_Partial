using System.Threading.Tasks;
using EmailService.Models;

namespace EmailService.Infrastructure
{
    public interface ISender
    {
        Task SendEmail(Message message);
    }
}