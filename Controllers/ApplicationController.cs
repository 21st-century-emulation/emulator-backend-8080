using System.Threading.Tasks;
using emulator_backend_8080.Models;
using emulator_backend_8080.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace emulator_backend_8080.Controllers
{
    [Route("api/v1")]
    public class ApplicationController : Controller
    {
        private readonly ILogger<ApplicationController> _logger;
        private readonly FetchExecuteServiceQueue _fetchExecuteServiceQueue;

        public ApplicationController(ILogger<ApplicationController> logger, FetchExecuteServiceQueue fetchExecuteServiceQueue)
        {
            _logger = logger;
            _fetchExecuteServiceQueue = fetchExecuteServiceQueue;
        }

        [HttpPost, Route("start")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> Start(string id)
        {
            _logger.LogInformation("Starting computer with id {0}", id);
            if (!(await _fetchExecuteServiceQueue.SetComputerToWaitingAsync(id)))
            {
                _logger.LogError("Weird deadlock issue getting lock to set computer to waiting");
                return StatusCode(500);
            }
            return Ok();
        }

        [HttpGet, Route("state")]
        [ProducesResponseType(typeof(Cpu), StatusCodes.Status200OK)]
        public async Task<IActionResult> Status(string id)
        {
            _logger.LogInformation("Requesting state of compuer with id {0}", id);
            var cpu = await _fetchExecuteServiceQueue.GetStateAsync();
            if (!cpu.HasValue) return StatusCode(500, "Couldn't acquire lock to get state");

            _logger.LogWarning("{0}", cpu?.Id);
            return Ok(cpu);
        }
    }
}
