using System;
using Microsoft.AspNetCore.Mvc;
using Monitoring;

namespace EmailService.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class HealthController : ControllerBase
    {
        [HttpGet]
        public StatusCodeResult HealthCheck()
        {
            MonitorService.Log.Debug("Health check passed successfully");
            return StatusCode(200);
        }
    }
}
