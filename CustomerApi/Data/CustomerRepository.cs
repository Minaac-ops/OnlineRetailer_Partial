using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;
using CustomerApi.Models;
using Monitoring;

namespace CustomerApi.Data
{
    public class CustomerRepository : IRepository<Customer>
    {
        private readonly CustomerApiContext db;

        public CustomerRepository(CustomerApiContext context)
        {
            db = context;
        }

        async Task<Customer> IRepository<Customer>.Get(int? id)
        {
            MonitorService.Log.Here().Debug("CustomerRepository: Get");
            var customer = await db.Customers.FirstOrDefaultAsync(c => c.Id == id);
            return customer;
        }

        async Task<IEnumerable<Customer>> IRepository<Customer>.GetAll()
        {
            
            MonitorService.Log.Here().Debug("CustomerRepository: GetAll");
            var select = await db.Customers.Select(customer => new Customer()
            {
                Id = customer.Id,
                CompanyName = customer.CompanyName,
                Email = customer.Email,
                PhoneNo = customer.PhoneNo,
                BillingAddress = customer.BillingAddress,
                ShippingAddress = customer.ShippingAddress
            }).ToListAsync();
            if (select == null)
            {
                throw new Exception("Customers couldn't be fetched");
            }

            return select;
        }

        async Task<Customer> IRepository<Customer>.Add(Customer entity)
        {
            MonitorService.Log.Here().Debug("CustomerRepository: Add");
            var newCustomer = await db.Customers.AddAsync(entity);
            entity.BillingAddress ??= entity.ShippingAddress;

            if (newCustomer == null)
            {
                throw new Exception("Customer couldn't be added to the database.");
            }

            await db.SaveChangesAsync();
            return newCustomer.Entity;
        }

        async Task<Customer> IRepository<Customer>.Edit(int id, Customer entity)
        {
            MonitorService.Log.Here().Debug("CustomerRepository: Edit");
            var customerToUpdate = await db.Customers.FirstOrDefaultAsync(c => c.Id == id);
            customerToUpdate.Email = entity.Email;
            customerToUpdate.BillingAddress = entity.BillingAddress;
            customerToUpdate.ShippingAddress = entity.ShippingAddress;

            db.Customers.Entry(customerToUpdate).State = EntityState.Modified;
            await db.SaveChangesAsync();
            return customerToUpdate;
        }

        public async Task Remove(int id)
        {
            throw new System.NotImplementedException();
        }
    }
}