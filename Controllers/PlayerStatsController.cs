using CS2_Surf_NET_API.Data;
using Microsoft.AspNetCore.Mvc;
using SurfTimer.Shared.Entities;
using SurfTimer.Shared.Sql;

namespace CS2_Surf_NET_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PlayerStatsController : ControllerBase
    {
        private readonly ILogger<PlayerStatsController> _logger;
        private readonly IDatabaseService _db;

        public PlayerStatsController(ILogger<PlayerStatsController> logger, IDatabaseService db)
        {
            _logger = logger;
            _db = db;
        }

        [ProducesResponseType(typeof(List<PlayerStatsRunData>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [HttpGet("playerId={playerId:int}&mapId={mapId:int}")]
        [EndpointSummary("All data for the player runs on a map.")]
        public async Task<ActionResult<List<MapTimeRunData>>> GetPlayerMapTimes(int playerId, int mapId)
        {
            try
            {
                var mapRuns = await _db.QueryAsync<PlayerStatsRunData>(
                    Queries.DB_QUERY_PS_GET_ALL_RUNTIMES,
                    new { playerId, mapId }
                );

                if (mapRuns is null)
                {
                    return NotFound();
                }

                _logger.LogInformation("Retrieved MapTimes for PlayerID '{PlayerId}' and MapID '{MapId}'", playerId, mapId);

                return Ok(mapRuns);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching Runs for PlayerID = {PlayerID} and MapID = {MapID}", playerId, mapId);
                return StatusCode(500, "Internal server error");
            }
        }
    }
}
