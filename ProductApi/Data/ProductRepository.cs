using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using Shared;

namespace ProductApi.Data
{
    public class ProductRepository : IRepository<Product>
    {
        private readonly ProductApiContext db;

        public ProductRepository(ProductApiContext context)
        {
            db = context;
        }

        Product IRepository<Product>.Add(Product entity)
        {
            var newProduct = db.Products.Add(entity).Entity;
            db.SaveChanges();
            return newProduct;
        }

        async void IRepository<Product>.Edit(int id,Product entity)
        {
            var productToModify = await db.Products.FindAsync(id);

            if (productToModify == null) return;
            
            productToModify.Name = entity.Name;
            productToModify.Price = entity.Price;
            productToModify.ItemsInStock = entity.ItemsInStock;
            productToModify.ItemsReserved = entity.ItemsReserved;
            
            db.Entry(productToModify).State = EntityState.Modified;
            await db.SaveChangesAsync();
        }

        Product IRepository<Product>.Get(int id)
        {
            return db.Products.FirstOrDefault(p => p.Id == id);
        }

        IEnumerable<Product> IRepository<Product>.GetAll()
        {
            return db.Products.ToList();
        }

        void IRepository<Product>.Remove(int id)
        {
            var product = db.Products.FirstOrDefault(p => p.Id == id);
            db.Products.Remove(product);
            db.SaveChanges();
        }
    }
}
