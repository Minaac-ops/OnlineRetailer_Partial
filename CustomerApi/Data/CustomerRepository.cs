using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using Shared;

namespace CustomerApi.Data
{
    public class CustomerRepository : IRepository<Customer>
    {
        private readonly CustomerApiContext db;

        public CustomerRepository(CustomerApiContext context)
        {
            db = context;
        }
        
        Customer IRepository<Customer>.Get(int id)
        {
            return db.Customers.FirstOrDefault(o => o.Id == id);
        }

        IEnumerable<Customer> IRepository<Customer>.GetAll()
        {
            var select = db.Customers.Select(customer => new Customer()
            {
                Id = customer.Id,
                CompanyName = customer.CompanyName,
                Email = customer.Email,
                PhoneNo = customer.PhoneNo,
                BillingAddress = customer.BillingAddress,
                ShippingAddress = customer.ShippingAddress
            });
            return select.ToList();
        }

        Customer IRepository<Customer>.Add(Customer entity)
        {
            var newCustomer = db.Customers.Add(entity).Entity;
            if (entity.BillingAddress==null)
            {
                entity.BillingAddress = entity.ShippingAddress;
            }
            
            db.SaveChanges();
            return newCustomer;
        }

        public async void Edit(int id,Customer entity)
        {
            var customerToUpdate = await db.Customers.FindAsync(id);

            if (customerToUpdate == null) return;
            customerToUpdate.Email = entity.Email;
            customerToUpdate.BillingAddress = entity.BillingAddress;
            customerToUpdate.ShippingAddress = entity.ShippingAddress;
            
            db.Entry(customerToUpdate).State = EntityState.Modified;
            await db.SaveChangesAsync();
        }

        public void Remove(int id)
        {
            throw new System.NotImplementedException();
        }
    }
}