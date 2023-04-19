using System.Collections.Generic;
using System.Threading.Tasks;

namespace ProductApi.Data
{
    public interface IRepository<T>
    {
        Task<IEnumerable<T>> GetAll();
        Task<T> Get(int id);
        Task<T> Add(T entity);
        Task Edit(T entity);
        Task Remove(int id);
    }
}
