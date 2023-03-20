using System.Collections.Generic;
using Shared;

namespace OrderApi.Infrastructure
{
    public interface IMessagePublisher
    {
        public void Dispose();

        public void PublishOrderStatusChangedMessage(int customerId, IList<OrderLine> orderlines, string topic);


    }
}