using Microsoft.AspNetCore.Mvc;
using SurfTimer.Api.Data;
using SurfTimer.Shared.DTO;
using SurfTimer.Shared.Entities;
using SurfTimer.Shared.Sql;

namespace SurfTimer.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class MapController : ControllerBase
    {
        private readonly ILogger<MapController> _logger;
        private readonly IDatabaseService _db;

        public MapController(ILogger<MapController> logger, IDatabaseService db)
        {
            _logger = logger;
            _db = db;
        }

        [ProducesResponseType(typeof(MapEntity), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [HttpGet("mapName={mapName}")]
        [EndpointSummary("Get the information about the Map. ID, Tier, etc.")]
        public async Task<ActionResult<MapEntity>> GetMapInfo(string mapName)
        {
            try
            {
                var mapInfo = await _db.QueryFirstOrDefaultAsync<MapEntity>(
                    Queries.DB_QUERY_MAP_GET_INFO,
                    new { mapName }
                );

                if (mapInfo is null)
                {
                    return NoContent();
                }

                _logger.LogInformation("Retrieved Map Info for {MapName} with ID {ID}", mapName, mapInfo.ID);

                return Ok(mapInfo);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching Map Info for '{MapName}'.", mapName);
                return StatusCode(500, "Internal server error");
            }
        }

        [ProducesResponseType(typeof(PostResponseEntity), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [HttpPost]
        [EndpointSummary("Add a new Map entry")]
        public async Task<ActionResult<PostResponseEntity>> InsertMapInfo([FromBody] MapDto mapDto)
        {
            // Matching named parameters in SQL
            var sqlParameters = new
            {
                mapDto.Name,
                mapDto.Author,
                mapDto.Tier,
                mapDto.Stages,
                mapDto.Bonuses,
                mapDto.Ranked,
                DateAdded = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds(),
                LastPlayed = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds()
            };

            try
            {
                var insertedId = await _db.ExecuteAsync(Queries.DB_QUERY_MAP_INSERT_INFO, sqlParameters);
                return CreatedAtAction(nameof(InsertMapInfo), new { id = insertedId }, new PostResponseEntity { Id = (int)insertedId });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = ex.Message });
            }
        }

        [ProducesResponseType(typeof(PostResponseEntity), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [HttpPut("mapId={id=int}")]
        [EndpointSummary("Update the information about the Map. Tier, Stages, Bonuses, etc.")]
        public async Task<ActionResult<PostResponseEntity>> UpdateMapInfo([FromBody] MapDto mapDto, int id)
        {
            // Matching named parameters in SQL
            var sqlParameters = new
            {
                LastPlayed = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds(),
                mapDto.Stages,
                mapDto.Bonuses,
                mapDto.Author,
                mapDto.Tier,
                mapDto.Ranked,
                id
            };

            try
            {
                var updatedId = await _db.ExecuteAsync(Queries.DB_QUERY_MAP_UPDATE_INFO_FULL, sqlParameters);
                return CreatedAtAction(nameof(UpdateMapInfo), new { id = updatedId }, new PostResponseEntity { Id = (int)updatedId });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = ex.Message });
            }
        }

        [ProducesResponseType(typeof(MapTimeRunDataEntity), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [HttpGet("mapId={id:int}")]
        [EndpointSummary("Get all runs for the specified MapID")]
        public async Task<ActionResult<MapTimeRunDataEntity>> GetMapRuns(int id)
        {
            try
            {
                var mapRuns = await _db.QueryAsync<MapTimeRunDataEntity>(
                    Queries.DB_QUERY_MAP_GET_RECORD_RUNS_AND_COUNT,
                    new { id }
                );

                if (mapRuns is null)
                {
                    return NoContent();
                }

                //foreach (var run in mapRuns)
                //{
                //    _logger.LogInformation("Run: PlayerID={PlayerID}, RunID={RunID}, Name={Name}, ReplayFrames={ReplayFrames}", run.PlayerID, run.ID, run.Name, run.ReplayFrames?.ToString().Length);
                //    //run.ReplayFramesBase64 = Convert.ToBase64String(run.ReplayFrames!);
                //}


                _logger.LogInformation("Retrieved all Map Runs for Map ID {ID}. Total {Total}", id, mapRuns.Count());

                return Ok(mapRuns);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching Map Runs for Map ID '{ID}'.", id);
                return StatusCode(500, "Internal server error");
            }
        }
    }
}
