using System.Collections.Generic;
using System.Threading.Tasks;
using Shared;

namespace OrderApi.Infrastructure
{
    public interface IMessagePublisher
    {
        Task PublishOrderCreatedMessage(int? customerId, int orderId,
            IList<OrderLine> orderLines);
        Task CreditStandingChangedMessage(int orderResultCustomerId);
        Task OrderStatusChangedMessage(int id, IList<OrderLine> orderLines, string topic);
        Task PublishOrderAccepted(int orderCustomerId, int orderId);
        Task PublishOrderCancelled(int orderCustomerId, int orderId);
        Task PublishOrderShippedEmail(int customerId, int orderId);
    }
}