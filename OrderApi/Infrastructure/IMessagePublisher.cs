using System.Collections.Generic;
using Shared;

namespace OrderApi.Infrastructure
{
    public interface IMessagePublisher
    {
        void PublishOrderCreatedMessage(int? customerId, int orderId,
            IList<OrderLine> orderLines);
        void CreditStandingChangedMessage(int orderResultCustomerId);
        void OrderStatusChangedMessage(int id, IList<OrderLine> orderLines, string topic);
        void PublishOrderAccepted(int orderCustomerId, int orderId);
        void PublishOrderCancelled(int orderCustomerId, int orderId);
        void PublishOrderShippedEmail(int customerId, int orderId);
    }
}