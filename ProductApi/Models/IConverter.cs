using Shared;

namespace ProductApi.Models
{
    public interface IConverter<T,G>
    {
        public Product Convert(ProductDto sharedProduct);

        public ProductDto Convert(Product hiddenProduct);
    }
}