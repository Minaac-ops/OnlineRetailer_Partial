
using Shared;

namespace OrderApi.Models
{
    public class OrderConverter : IConverter<Order,OrderDto>
    {
        public Order Convert(OrderDto sharedProduct)
        {
            return new Order()
            {
                Id = sharedProduct.Id,
                CustomerId = sharedProduct.CustomerId,
                Date = sharedProduct.Date,
                OrderLines = sharedProduct.OrderLines,
                Status = sharedProduct.Status,
            };
        }

        public OrderDto Convert(Order hiddenProduct)
        {
            return new OrderDto()
            {
                Id = hiddenProduct.Id,
                CustomerId = hiddenProduct.CustomerId,
                Date = hiddenProduct.Date,
                OrderLines = hiddenProduct.OrderLines,
                Status = hiddenProduct.Status,
            };
        }
    }
}