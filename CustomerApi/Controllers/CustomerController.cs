using System;
using System.Threading.Tasks;
using CustomerApi.Data;
using Microsoft.AspNetCore.Mvc;
using Shared;

namespace CustomerApi.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class CustomerController : ControllerBase
    {
        private readonly IRepository<Customer> _repository;

        public CustomerController(IRepository<Customer> repos)
        {
            _repository = repos;
        }

        [HttpPost]
        public async Task<Customer> Post([FromBody] Customer customer)
        {
            try
            {
                if (customer==null)
                {
                    throw new Exception("Customer can't be null.");
                }
                var newCustomer = await _repository.Add(customer);
                return newCustomer;
            }
            catch (Exception e)
            {
                throw new Exception("Customer was not created due to error " + e.Message);
            }
        }

        // GET orders/5
        [HttpGet("{id}", Name = "GetCustomer")]
        public async Task<Customer> Get(int id)
        {
            try
            {
                var item = await _repository.Get(id);
                return item;
            }
            catch (Exception e)
            {
                throw new Exception("Customer with id " + id+" couldn't be found due to error " +e.Message);
            }
        }

        [HttpPut("{id}")]
        public async Task Put(int id, [FromBody] Customer customer)
        {
            try
            {
                await _repository.Edit(id,customer);
            }
            catch (Exception e)
            {
                throw new Exception("Customer with id "+ id+" could not be updated due to error " + e.Message);
            }
        }
    }
}
