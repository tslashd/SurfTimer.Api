using CS2_Surf_NET_API.Data;
using Microsoft.AspNetCore.Mvc;
using SurfTimer.Shared.DTO;
using SurfTimer.Shared.Entities;
using SurfTimer.Shared.Sql;

namespace CS2_Surf_NET_API.Controllers
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

        [ProducesResponseType(typeof(Map), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [HttpGet("mapName={mapName}")]
        [EndpointSummary("Get the information about the Map. ID, Tier, etc.")]
        public async Task<ActionResult<Map>> GetMapInfo(string mapName)
        {
            try
            {
                var mapInfo = await _db.QueryFirstOrDefaultAsync<Map>(
                    Queries.DB_QUERY_MAP_GET_INFO,
                    new { mapName }
                );

                if (mapInfo is null)
                {
                    return NotFound();
                }

                _logger.LogInformation("Retrieved Map Info for {MapName} with ID {ID}", mapName, mapInfo.ID);

                return Ok(mapInfo);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching Map Info for '{MapName}' profile.", mapName);
                return StatusCode(500, "Internal server error");
            }
        }

        [ProducesResponseType(typeof(PostResponseDto), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [HttpPost]
        [EndpointSummary("Add a new Map entry")]
        public async Task<ActionResult<PostResponseDto>> InsertMapInfo([FromBody] MapDto mapDto)
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
                return CreatedAtAction(nameof(InsertMapInfo), new { id = insertedId }, new PostResponseDto { Id = (int)insertedId });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = ex.Message });
            }
        }

        [ProducesResponseType(typeof(PostResponseDto), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [HttpPut("mapId={id=int}")]
        [EndpointSummary("Update the information about the Map. Tier, Stages, Bonuses, etc.")]
        public async Task<ActionResult<PostResponseDto>> UpdateMapInfo([FromBody] MapDto mapDto, int id)
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
                return CreatedAtAction(nameof(UpdateMapInfo), new { id = updatedId }, new PostResponseDto { Id = (int)updatedId });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = ex.Message });
            }
        }

        [ProducesResponseType(typeof(MapTimeRunData), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [HttpGet("mapId={id:int}")]
        [EndpointSummary("Get all runs for the specified MapID")]
        public async Task<ActionResult<MapTimeRunData>> GetMapRuns(int id)
        {
            try
            {
                var mapRuns = await _db.QueryAsync<MapTimeRunData>(
                    Queries.DB_QUERY_MAP_GET_RECORD_RUNS_AND_COUNT,
                    new { id }
                );

                if (mapRuns is null)
                {
                    return NotFound();
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
                _logger.LogError(ex, "Error fetching Map Runs for Map ID '{ID}' profile.", id);
                return StatusCode(500, "Internal server error");
            }
        }
    }
}
