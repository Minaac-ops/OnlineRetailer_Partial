using System.Collections.Generic;
using Shared;

namespace CustomerApi.Data
{
    public interface IRepository<T>
    {
        IEnumerable<T> GetAll();
        T Get(int id);
        T Add(T entity);
        void Edit(int id, Customer customer);
        void Remove(int id);
    }
}