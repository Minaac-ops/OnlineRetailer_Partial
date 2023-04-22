using System;
using System.Threading.Tasks;
using CustomerApi.Data;
using CustomerApi.Models;
using Microsoft.AspNetCore.Mvc;
using Shared;

namespace CustomerApi.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class CustomerController : ControllerBase
    {
        private readonly IRepository<Customer> _repository;
        private readonly IConverter<Customer,CustomerDto> _converter;

        public CustomerController(IRepository<Customer> repos,IConverter<Customer,CustomerDto> converter)
        {
            _converter = converter;
            _repository = repos;
        }

        [HttpPost]
        public async Task<CustomerDto> Post([FromBody] CustomerDto customerDto)
        {
            try
            {
                if (customerDto==null)
                {
                    throw new Exception("Customer can't be null.");
                }
                var newCustomer = await _repository.Add(_converter.Convert(customerDto));
                return _converter.Convert(newCustomer);
            }
            catch (Exception e)
            {
                throw new Exception("Customer was not created due to error " + e.Message);
            }
        }

        // GET orders/5
        [HttpGet("{id}", Name = "GetCustomer")]
        public async Task<CustomerDto> Get(int id)
        {
                try
                {
                    var item = await _repository.Get(id);
                
                    return _converter.Convert(item);
                }
                catch (Exception e)
                {
                    throw new Exception("Customer with id " + id+" couldn't be found due to error " +e.Message);
                }
            }

        [HttpPut("{id}")]
        public async Task<CustomerDto> Put(int id, [FromBody] CustomerDto customer)
        {
            try
            {
                var updatedCustomer = await _repository.Edit(id,_converter.Convert(customer));
                return _converter.Convert(updatedCustomer);
            }
            catch (Exception e)
            {
                throw new Exception("Customer with id "+ id+" could not be updated due to error " + e.Message);
            }
        }
    }
}
