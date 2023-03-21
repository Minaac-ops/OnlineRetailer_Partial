

using Shared;

namespace OrderApi.Models
{
    public interface IConverter<T,G>
    {
        public Order Convert(OrderDto sharedProduct);

        public OrderDto Convert(Order hiddenProduct);
    }
}