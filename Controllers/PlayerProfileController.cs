using CS2_Surf_NET_API.Data;
using Microsoft.AspNetCore.Mvc;
using SurfTimer.Shared.DTO;
using SurfTimer.Shared.Entities;
using SurfTimer.Shared.Sql;

namespace CS2_Surf_NET_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PlayerProfileController : ControllerBase
    {
        private readonly ILogger<PlayerProfileController> _logger;
        private readonly IDatabaseService _db;

        public PlayerProfileController(ILogger<PlayerProfileController> logger, IDatabaseService db)
        {
            _logger = logger;
            _db = db;
        }

        [ProducesResponseType(typeof(List<PlayerProfile>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [HttpGet]
        [EndpointSummary("Get all Player profiles")]
        public async Task<ActionResult<List<PlayerProfile>>> GetAllProfiles()
        {
            const string sql = "SELECT * FROM Player;";

            try
            {
                var players = await _db.QueryAsync<PlayerProfile>(sql);
                return Ok(players);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching player profiles.");
                return StatusCode(500, "Internal server error");
            }
        }

        [ProducesResponseType(typeof(PlayerProfile), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [HttpGet("steamId={SteamID:long}")]
        [EndpointSummary("Get the Player profile data for a specific SteamID")]
        public async Task<ActionResult<PlayerProfile>> GetProfile(ulong SteamID)

        {
            try
            {
                var player = await _db.QueryFirstOrDefaultAsync<PlayerProfile>(
                    Queries.DB_QUERY_PP_GET_PROFILE,
                    new { SteamID }
                );
                return player is not null ? Ok(player) : NotFound();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching player {SteamID} profile.", SteamID);
                return StatusCode(500, "Internal server error");
            }
        }

        [ProducesResponseType(typeof(PostResponseDto), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [HttpPost]
        [EndpointSummary("Add a new Player profile")]
        public async Task<ActionResult<PostResponseDto>> InsertProfile([FromBody] PlayerProfileDto profileDto)
        {
            // Matching named parameters in SQL
            var sqlParameters = new
            {
                profileDto.Name,
                profileDto.SteamID,
                profileDto.Country,
                JoinDate = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds(),
                LastSeen = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds(),
                profileDto.Connections
            };

            try
            {
                var insertedId = await _db.ExecuteAsync(Queries.DB_QUERY_PP_INSERT_PROFILE, sqlParameters);
                return CreatedAtAction(nameof(InsertProfile), new { id = insertedId }, new PostResponseDto { Id = (int)insertedId });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = ex.Message });
            }
        }

        [ProducesResponseType(typeof(PostResponseDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [HttpPut("playerId={id:int}")]
        [EndpointSummary("Update the data of a specific Player profile")]
        public async Task<ActionResult<PostResponseDto>> UpdateProfile([FromBody] PlayerProfileDto profileDto, int id)
        {
            // Matching named parameters in SQL
            var sqlParameters = new
            {
                profileDto.Country,
                LastSeen = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds(),
                profileDto.Name,
                id
            };

            try
            {
                var insertedId = await _db.ExecuteAsync(Queries.DB_QUERY_PP_UPDATE_PROFILE, sqlParameters);
                return CreatedAtAction(nameof(InsertProfile), new { id = insertedId }, new PostResponseDto { Id = (int)insertedId });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = ex.Message });
            }
        }

        [ProducesResponseType(typeof(PostResponseDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [HttpDelete("playerId={id:int}")]
        [EndpointSummary("Delete the profile of a specific Player ID")]
        public async Task<ActionResult<PostResponseDto>> DeleteProfile(int id)
        {
            try
            {
                var rowsAffected = await _db.ExecuteAsync(
                    Queries.DB_QUERY_PP_DELETE_PROFILE,
                    new { Id = id }
                );

                if (rowsAffected == 0)
                {
                    return Ok(new { message = $"Player profile with ID {id} deleted.", id = $"{id}" });
                }
                else
                {
                    return NotFound(new { message = $"Player profile with ID {id} not found." });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting player profile with ID {ID}.", id);
                return StatusCode(500, new { error = "Internal server error" });
            }
        }

    }
}
