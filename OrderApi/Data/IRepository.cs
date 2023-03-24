using System.Collections.Generic;
using System.Threading.Tasks;

namespace OrderApi.Data
{
    public interface IRepository<T>
    {
        IEnumerable<T> GetAll();
        T Get(int id);
        T Add(T entity);
        void Edit(int id,T entity);
        void Remove(int id);
        IEnumerable<T> GetByCustomerId(int customerId);
    }
}
