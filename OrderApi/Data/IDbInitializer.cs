using System.Threading.Tasks;

namespace OrderApi.Data
{
    public interface IDbInitializer
    {
        Task Initialize(OrderApiContext context);
    }
}
