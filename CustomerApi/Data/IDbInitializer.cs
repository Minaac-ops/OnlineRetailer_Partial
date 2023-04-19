using System.Threading.Tasks;

namespace CustomerApi.Data
{
    public interface IDbInitializer
    {
        Task Initialize(CustomerApiContext context);
    }
}