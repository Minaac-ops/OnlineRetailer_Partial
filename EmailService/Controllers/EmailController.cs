using System;
using System.Threading.Tasks;
using EmailService.Infrastructure;
using EmailService.Models;
using Microsoft.AspNetCore.Mvc;
using NuGet.Protocol;
using RestSharp;
using Shared;

namespace EmailService.Controllers
{

    [Route("api/[controller]")]
    [ApiController]
    public class EmailController : ControllerBase
    {

        private IEmailSender _emailSender;

        public EmailController(IEmailSender emailSender)
        {
            _emailSender = emailSender;
        }


        [HttpGet]
        public async Task<string> Get()
        {
            var message = new Message(new string[] {"customer.Email"},"customer.CompanyName", "testemail", "This is the contect");
            Console.WriteLine(message.To);
            await _emailSender.SendEmail(message);
            return "message send";
        }
    }
}
