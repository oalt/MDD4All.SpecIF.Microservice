using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Logging;
using System.Net;
using System.Threading.Tasks;


namespace MDD4All.SpecIF.Microservice.Controllers
{   

    /// <summary>
    /// Health Check for SpecIF Backend
    /// </summary>
    [ApiController]
    [ApiVersion("1.1")]
    [Route("specif/v{version:apiVersion}/health")]
    [ApiExplorerSettings(IgnoreApi = true)]
    public class HealthController : ControllerBase
    {
        private readonly ILogger<HealthController> _logger;
        private readonly HealthCheckService _service;

        public HealthController(ILogger<HealthController> logger, HealthCheckService service)
        {
            _logger = logger;
            _service = service;
        }

        [HttpGet]
        public async Task<IActionResult> Get()
        {
            ActionResult result;
            HealthReport report = await _service.CheckHealthAsync();

            _logger.LogInformation($"Get Health Information: {report}");
            if (report.Status == HealthStatus.Healthy)
            {
                result = Ok(report);
            }
            else
            {
                result = StatusCode((int)HttpStatusCode.ServiceUnavailable, report);
            }
            return result;
        }
    }
}