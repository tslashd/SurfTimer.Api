using CS2_Surf_NET_API.Data;
using Microsoft.AspNetCore.Mvc;
using SurfTimer.Shared.Entities;
using SurfTimer.Shared.Sql;

namespace CS2_Surf_NET_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PersonalBestController : ControllerBase
    {
        private readonly ILogger<PersonalBestController> _logger;
        private readonly IDatabaseService _db;

        public PersonalBestController(ILogger<PersonalBestController> logger, IDatabaseService db)
        {
            _logger = logger;
            _db = db;
        }

        [ProducesResponseType(typeof(Dictionary<int, Checkpoint>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [HttpGet("checkpoints/mapTimeId={mapTimeId:int}")]
        [EndpointSummary("Get all Checkpoints data for a specific MapTimeID")]
        public async Task<ActionResult<Dictionary<int, Checkpoint>>> GetMapRunCheckpoints(int mapTimeId)
        {
            try
            {
                var checkpoints = await _db.QueryAsync<Checkpoint>(
                    Queries.DB_QUERY_PB_GET_CPS,
                    new { mapTimeId }
                );

                if (checkpoints is null)
                {
                    return NotFound();
                }

                _logger.LogInformation("Retrieved Checkpoints for MapTimeID {ID}", mapTimeId);

                var checkpointDict = checkpoints.ToDictionary(cp => cp.CP);

                return Ok(checkpointDict);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching Checkpoints for MapTimeID {ID}", mapTimeId);
                return StatusCode(500, "Internal server error");
            }
        }

        [ProducesResponseType(typeof(List<MapTimeRunData>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [HttpGet("playerId={playerId:int}&mapId={mapId:int}&type={type:int}&style={style:int}")]
        [EndpointSummary("Get all runs for the PlayerID, MapID, Type and Style combination")]
        public async Task<ActionResult<List<MapTimeRunData>>> GetMapRunsByPlayer(int playerId, int mapId, int type, int style)
        {
            try
            {
                var mapRuns = await _db.QueryAsync<MapTimeRunData>(
                    Queries.DB_QUERY_PB_GET_TYPE_RUNTIME,
                    new { playerId, mapId, type, style }
                );

                if (mapRuns is null)
                {
                    return NotFound();
                }

                _logger.LogInformation("Retrieved all MapTimes data ({RunsCount}) for PlayerID {PlayerID}, MapID {MapID}, Type {Type} and Style {Style}", mapRuns.Count(), playerId, mapId, type, style);

                return Ok(mapRuns);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching Runs for PlayerID {PlayerID} and MapID {MapID}", playerId, mapId);
                return StatusCode(500, "Internal server error");
            }
        }

        [ProducesResponseType(typeof(MapTimeRunData), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [HttpGet("runById/mapTimeId={mapTimeId:int}")]
        [EndpointSummary("Get a specific MapTime by it's ID")]
        public async Task<ActionResult<List<MapTimeRunData>>> GetMapRunById(int mapTimeId)
        {
            try
            {
                var mapRun = await _db.QueryFirstOrDefaultAsync<MapTimeRunData>(
                    Queries.DB_QUERY_PB_GET_SPECIFIC_MAPTIME_DATA,
                    new { mapTimeId }
                );

                if (mapRun is null)
                {
                    return NotFound();
                }

                _logger.LogInformation("Retrieved MapTime data for RunID {RunID}", mapTimeId);

                return Ok(mapRun);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching Run with ID {RunID}", mapTimeId);
                return StatusCode(500, "Internal server error");
            }
        }
    }
}
