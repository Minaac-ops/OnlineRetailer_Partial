using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Monitoring;

namespace ProductApi.Controllers
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
