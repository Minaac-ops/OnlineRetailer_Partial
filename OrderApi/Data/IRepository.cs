using System.Collections.Generic;
using System.Threading.Tasks;

namespace OrderApi.Data
{
    public interface IRepository<T>
    {
        Task<IEnumerable<T>> GetAll();
        Task<T> Get(int id);
        Task<T> Add(T entity);
        void Edit(T entity);
        void Remove(int id);
        Task<IEnumerable<T>> GetByCustomerId(int customerId);
    }
}
