using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CustomerApi.Data;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
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
        public IActionResult Post([FromBody] Customer customer)
        {
            if (customer == null)
            {
                return BadRequest();
            }

            var newCustomer = _repository.Add(customer);
            if (newCustomer!=null)
            {
                return CreatedAtRoute(new {id = newCustomer.Id}, newCustomer);
            }

            return NoContent();
        }

        // GET orders/5
        [HttpGet("{id}", Name = "GetCustomer")]
        public IActionResult Get(int id)
        {
            var item = _repository.Get(id);
            if (item == null)
            {
                return NotFound();
            }
            return new ObjectResult(item);
        }

        [HttpPut("{id}")]
        public IActionResult Put(int id, [FromBody] Customer customer)
        {
            if (customer == null)
            {
                return BadRequest();
            }

            _repository.Edit(id,customer);

            return new NoContentResult();
        }
    }
}
