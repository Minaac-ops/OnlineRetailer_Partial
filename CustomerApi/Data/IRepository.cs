using System.Collections.Generic;
using System.Threading.Tasks;
using Shared;

namespace CustomerApi.Data
{
    public interface IRepository<T>
    {
        Task<IEnumerable<T>> GetAll();
        Task<T> Get(int id);
        Task<T> Add(T entity);
        Task Edit(int id, T Customer);
        void Remove(int id);
    }
}