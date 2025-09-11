using Microsoft.AspNetCore.Mvc;
using SurfTimer.Shared.Data;
using SurfTimer.Shared.Entities;
using SurfTimer.Shared.Sql;

namespace SurfTimer.Api.Controllers
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

        [ProducesResponseType(typeof(List<MapTimeRunDataEntity>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [HttpGet("playerId={playerId:int}&mapId={mapId:int}")]
        [EndpointSummary("All data for the player runs on a map.")]
        public async Task<ActionResult<List<MapTimeRunDataEntity>>> GetPlayerMapTimes(
            int playerId,
            int mapId
        )
        {
            try
            {
                var mapRuns = await _db.QueryAsync<MapTimeRunDataEntity>(
                    Queries.DB_QUERY_PS_GET_ALL_RUNTIMES,
                    new { playerId, mapId }
                );

                if (mapRuns is null)
                {
                    return NoContent();
                }

                _logger.LogInformation(
                    "Retrieved MapTimes for PlayerID '{PlayerId}' and MapID '{MapId}'",
                    playerId,
                    mapId
                );

                return Ok(mapRuns);
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Error fetching Runs for PlayerID = {PlayerID} and MapID = {MapID}",
                    playerId,
                    mapId
                );
                return StatusCode(500, "Internal server error");
            }
        }
    }
}
