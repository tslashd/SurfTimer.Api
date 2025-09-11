using Microsoft.AspNetCore.Mvc;
using SurfTimer.Shared.Data;
using SurfTimer.Shared.DTO;
using SurfTimer.Shared.Entities;
using SurfTimer.Shared.Sql;

namespace SurfTimer.Api.Controllers
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

        [ProducesResponseType(typeof(List<PlayerProfileEntity>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [HttpGet]
        [EndpointSummary("Get all Player profiles")]
        public async Task<ActionResult<List<PlayerProfileEntity>>> GetAllProfiles()
        {
            const string sql = "SELECT * FROM Player;";

            try
            {
                var players = await _db.QueryAsync<PlayerProfileEntity>(sql);
                return Ok(players);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching player profiles.");
                return StatusCode(500, "Internal server error");
            }
        }

        [ProducesResponseType(typeof(PlayerProfileEntity), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [HttpGet("steamId={SteamID:long}")]
        [EndpointSummary("Get the Player profile data for a specific SteamID")]
        public async Task<ActionResult<PlayerProfileEntity>> GetProfile(ulong SteamID)
        {
            try
            {
                var player = await _db.QueryFirstOrDefaultAsync<PlayerProfileEntity>(
                    Queries.DB_QUERY_PP_GET_PROFILE,
                    new { SteamID }
                );
                return player is not null ? Ok(player) : NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching player {SteamID} profile.", SteamID);
                return StatusCode(500, "Internal server error");
            }
        }

        [ProducesResponseType(typeof(PostResponseEntity), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [HttpPost]
        [EndpointSummary("Add a new Player profile")]
        public async Task<ActionResult<PostResponseEntity>> InsertProfile(
            [FromBody] PlayerProfileDto profileDto
        )
        {
            // Matching named parameters in SQL
            var sqlParameters = new
            {
                profileDto.Name,
                profileDto.SteamID,
                profileDto.Country,
                JoinDate = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds(),
                LastSeen = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds(),
                profileDto.Connections,
            };

            try
            {
                var insertedId = await _db.InsertAsync(
                    Queries.DB_QUERY_PP_INSERT_PROFILE,
                    sqlParameters
                );
                return CreatedAtAction(
                    nameof(InsertProfile),
                    new { id = insertedId },
                    new PostResponseEntity { Id = (int)insertedId }
                );
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = ex.Message });
            }
        }

        [ProducesResponseType(typeof(PostResponseEntity), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [HttpPut("playerId={id:int}")]
        [EndpointSummary("Update the data of a specific Player profile")]
        public async Task<ActionResult<PostResponseEntity>> UpdateProfile(
            [FromBody] PlayerProfileDto profileDto,
            int id
        )
        {
            // Matching named parameters in SQL
            var sqlParameters = new
            {
                profileDto.Country,
                LastSeen = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds(),
                profileDto.Name,
                id,
            };

            try
            {
                var affectedRows = await _db.ExecuteAsync(
                    Queries.DB_QUERY_PP_UPDATE_PROFILE,
                    sqlParameters
                );
                return Ok(
                    new PostResponseEntity 
                    { 
                        Id = id, 
                        Affected = affectedRows 
                    }
                );
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = ex.Message });
            }
        }

        [ProducesResponseType(typeof(PostResponseEntity), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [HttpDelete("playerId={id:int}")]
        [EndpointSummary("Delete the profile of a specific Player ID")]
        public async Task<ActionResult<PostResponseEntity>> DeleteProfile(int id)
        {
            try
            {
                var rowsAffected = await _db.ExecuteAsync(
                    Queries.DB_QUERY_PP_DELETE_PROFILE,
                    new { Id = id }
                );

                if (rowsAffected == 0)
                {
                    return Ok(
                        new PostResponseEntity 
                        { 
                            Id = id, 
                            Affected = rowsAffected 
                        }
                    );
                }
                else
                {
                    return NoContent();
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
