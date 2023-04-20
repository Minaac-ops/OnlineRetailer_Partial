using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices.JavaScript;
using System.Threading.Tasks;
using EmailService.Infrastructure;
using EmailService.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace EmailService.Controllers
{

    [Route("api/[controller]")]
    [ApiController]
    public class EmailController : ControllerBase
    {

        private ISender _sender;

        public EmailController(ISender sender)
        {
            _sender = sender;
        }


        [HttpGet]
        public async Task<string> Get()
        {
            var message = new Message(new string[] {"sikypi@givmail.com"}, "testemail", "This is the contect");
            Console.WriteLine(message.To);
            await _sender.SendEmail(message);
            return "message send";
        }
    }
}
