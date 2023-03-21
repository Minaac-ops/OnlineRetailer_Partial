using Shared;

namespace CustomerApi.Models
{
    public interface IConverter<T,G>
    {
        public Customer Convert(CustomerDto sharedProduct);

        public CustomerDto Convert(Customer hiddenProduct);
    }
}