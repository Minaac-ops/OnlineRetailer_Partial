using System.Collections.Generic;
using System.Threading.Tasks;

namespace OrderApi.Data
{
    public interface IRepository<T>
    {
        Task<IEnumerable<T>> GetAll();
        Task<T> Get(int id);
        Task<T> Add(T entity);
        Task Edit(int id,T entity);
        Task Remove(int id);
    }
}
