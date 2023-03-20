using Shared;

namespace ProductApi.Models
{
    public class ProductConvert : IConverter<Product,ProductDto>
    {

        public Product Convert(ProductDto sharedProduct)
        {
            return new Product()
            {
                Id = sharedProduct.Id,
                ItemsInStock = sharedProduct.ItemsInStock,
                ItemsReserved = sharedProduct.ItemsReserved,
                Name = sharedProduct.Name,
                Price = sharedProduct.Price,
            };
        }

        public ProductDto Convert(Product hiddenProduct)
        {
            return new ProductDto()
            {
                Id = hiddenProduct.Id,
                ItemsInStock = hiddenProduct.ItemsInStock,
                ItemsReserved = hiddenProduct.ItemsReserved,
                Name = hiddenProduct.Name,
                Price = hiddenProduct.Price,
            };
        }
    }
}