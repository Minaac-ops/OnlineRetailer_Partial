using Shared;

namespace CustomerApi.Models
{
    public class CustomerConverter : IConverter<Customer,CustomerDto>
    {
        public Customer Convert(CustomerDto sharedProduct)
        {
            return new Customer()
            {
                Id = sharedProduct.Id,
                CompanyName = sharedProduct.CompanyName,
                BillingAddress = sharedProduct.BillingAddress,
                Email = sharedProduct.Email,
                PhoneNo = sharedProduct.PhoneNo,
                ShippingAddress = sharedProduct.ShippingAddress,
            };
        }

        public CustomerDto Convert(Customer hiddenProduct)
        {
            return new CustomerDto()
            {
                Id = hiddenProduct.Id,
                CompanyName = hiddenProduct.CompanyName,
                BillingAddress = hiddenProduct.BillingAddress,
                Email = hiddenProduct.Email,
                PhoneNo = hiddenProduct.PhoneNo,
                ShippingAddress = hiddenProduct.ShippingAddress,
            };
        }
    }
}