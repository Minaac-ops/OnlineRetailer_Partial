﻿using System.Collections.Generic;

namespace ProductApi.Data
{
    public interface IRepository<T>
    {
        IEnumerable<T> GetAll();
        T Get(int id);
        T Add(T entity);
        void Edit(int id,T entity);
        void Remove(int id);
    }
}
