using System.Collections.Generic;
using System.Threading.Tasks;
using Shared;

namespace CustomerApi.Data
{
    public interface IRepository<T>
    {
        Task<IEnumerable<T>> GetAll();
        Task<T> Get(int? id);
        Task<T> Add(T entity);
        Task<T> Edit(int id, T entity);
        Task Remove(int id);
    }
}