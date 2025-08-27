using Microsoft.AspNetCore.Mvc;

namespace SurfTimer.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UtilitiesController : ControllerBase
    {
        /// <summary>
        /// Endpoint used for checking connection and latency between client and API.
        /// </summary>
        /// <param name="client_unix">The client's UNIX timestamp in seconds.</param>
        /// <returns>Latency data in seconds and milliseconds.</returns>
        [ProducesResponseType(typeof(Dictionary<string, double>), StatusCodes.Status200OK)]
        [HttpGet("ping/clientUnix={clientUnix:double}")]
        public IActionResult Ping(double clientUnix)
        {
            double serverUnix = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() / 1000.0;
            double latency = serverUnix - clientUnix;

            var response = new
            {
                clientUnix,
                serverUnix,
                latencySeconds = latency,
                latencyMs = Math.Round(latency * 1000, 2),
            };

            return Ok(response);
        }
    }
}
