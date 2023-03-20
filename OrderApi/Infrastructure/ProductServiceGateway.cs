using System.Threading.Tasks;
using RestSharp;
using Shared;

namespace OrderApi.Infrastructure
{
    public class ProductServiceGateway : IServiceGateway<ProductDto>
    {
        private string productServiceBaseUrl;
        
        public ProductServiceGateway(string baseUrl)
        {
            productServiceBaseUrl = baseUrl;
        }
        
        public ProductDto Get(int id)
        {
            RestClient c = new RestClient(productServiceBaseUrl);

            var request = new RestRequest(id.ToString());
            var response = c.GetAsync<ProductDto>(request);
            response.Wait();
            return response.Result;
        }
    }
}