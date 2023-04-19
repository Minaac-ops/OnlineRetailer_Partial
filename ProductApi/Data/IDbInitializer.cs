using System.Threading.Tasks;

namespace ProductApi.Data
{
    public interface IDbInitializer
    {
        Task Initialize(ProductApiContext context);
    }
}
