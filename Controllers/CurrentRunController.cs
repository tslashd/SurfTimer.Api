using CS2_Surf_NET_API.Data;
using Dapper;
using Microsoft.AspNetCore.Mvc;
using SurfTimer.Shared.DTO;
using SurfTimer.Shared.Sql;

namespace CS2_Surf_NET_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CurrentRunController : ControllerBase
    {
        private readonly ILogger<CurrentRunController> _logger;
        private readonly IDatabaseService _db;

        public CurrentRunController(ILogger<CurrentRunController> logger, IDatabaseService db)
        {
            _logger = logger;
            _db = db;
        }

        [ProducesResponseType(typeof(PostResponseDto), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [HttpPost("saveMapTime")]
        [EndpointSummary("Add a new MapTime entry")]
        public async Task<ActionResult<PostResponseDto>> InsertMapTime([FromBody] MapTimeRunDataDto mapRunDataDto)
        {
            var runDate = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();

            try
            {
                int checkpointCount = 0;
                long insertedMapTimeId = 0;

                await _db.TransactionAsync(async (conn, trx) =>
                {
                    // 1. Insert into MapTimes (inside the transaction)
                    var sqlParameters = new
                    {
                        mapRunDataDto.PlayerID,
                        mapRunDataDto.MapID,
                        mapRunDataDto.Style,
                        mapRunDataDto.Type,
                        mapRunDataDto.Stage,
                        mapRunDataDto.RunTime,
                        mapRunDataDto.StartVelX,
                        mapRunDataDto.StartVelY,
                        mapRunDataDto.StartVelZ,
                        mapRunDataDto.EndVelX,
                        mapRunDataDto.EndVelY,
                        mapRunDataDto.EndVelZ,
                        RunDate = runDate,
                        mapRunDataDto.ReplayFrames
                    };

                    await conn.ExecuteAsync(Queries.DB_QUERY_CR_INSERT_TIME, sqlParameters, trx);

                    // Вземаме LAST_INSERT_ID() inside the SAME transaction
                    insertedMapTimeId = await conn.ExecuteScalarAsync<long>("SELECT LAST_INSERT_ID();", transaction: trx);

                    Console.WriteLine($"Inserted MapTime ID {insertedMapTimeId}");

                    // 2. If checkpoints are present, insert them
                    if (mapRunDataDto.Checkpoints != null && mapRunDataDto.Type == 0)
                    {
                        Console.WriteLine($"Moving onto inserting {mapRunDataDto.Checkpoints.Count} checkpoints");
                        foreach (var checkpointParams in from cp in mapRunDataDto.Checkpoints
                            let checkpointParams = new
                            {
                                MapTimeId = insertedMapTimeId,
                                cp.Value.CP,
                                cp.Value.RunTime,
                                cp.Value.StartVelX,
                                cp.Value.StartVelY,
                                cp.Value.StartVelZ,
                                cp.Value.EndVelX,
                                cp.Value.EndVelY,
                                cp.Value.EndVelZ,
                                cp.Value.Attempts,
                                cp.Value.EndTouch
                            }
                            select checkpointParams)
                        {
                            checkpointCount += await conn.ExecuteAsync(
                                Queries.DB_QUERY_CR_INSERT_CP,
                                checkpointParams,
                                transaction: trx
                            );
                        }
                    }
                });

                // All good
                return CreatedAtAction(nameof(InsertMapTime), new { id = insertedMapTimeId }, new PostResponseDto
                {
                    Id = (int)insertedMapTimeId,
                    Inserted = checkpointCount > 0 ? checkpointCount + 1 : 1,
                    Trx = checkpointCount > 0
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to insert map time and checkpoints.");
                return StatusCode(500, new { error = ex.Message });
            }
        }

        [ProducesResponseType(typeof(PostResponseDto), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [HttpPut("updateMapTime/mapTimeId={mapTimeId:int}")]
        [EndpointSummary("Update an existing MapTime entry")]
        public async Task<ActionResult<PostResponseDto>> UpdateMapTime([FromBody] MapTimeRunDataDto mapRunDataDto, int mapTimeId)
        {
            var runDate = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();

            try
            {
                int checkpointCount = 0;

                await _db.TransactionAsync(async (conn, trx) =>
                {
                    // 1. Insert into MapTimes (inside the transaction)
                    var sqlParameters = new
                    {
                        mapRunDataDto.RunTime,
                        mapRunDataDto.StartVelX,
                        mapRunDataDto.StartVelY,
                        mapRunDataDto.StartVelZ,
                        mapRunDataDto.EndVelX,
                        mapRunDataDto.EndVelY,
                        mapRunDataDto.EndVelZ,
                        RunDate = runDate,
                        mapRunDataDto.ReplayFrames,
                        mapTimeId
                    };

                    await conn.ExecuteAsync(Queries.DB_QUERY_CR_UPDATE_TIME, sqlParameters, trx);

                    Console.WriteLine($"Updated MapTime ID {mapTimeId}");

                    // 2. If checkpoints are present, insert them
                    if (mapRunDataDto.Checkpoints != null && mapRunDataDto.Type == 0)
                    {
                        Console.WriteLine($"Moving onto updating {mapRunDataDto.Checkpoints.Count} checkpoints");
                        foreach (var checkpointParams in from cp in mapRunDataDto.Checkpoints
                            let checkpointParams = new
                            {
                                MapTimeId = mapTimeId,
                                cp.Value.CP,
                                cp.Value.RunTime,
                                cp.Value.StartVelX,
                                cp.Value.StartVelY,
                                cp.Value.StartVelZ,
                                cp.Value.EndVelX,
                                cp.Value.EndVelY,
                                cp.Value.EndVelZ,
                                cp.Value.Attempts,
                                cp.Value.EndTouch
                            }
                            select checkpointParams)
                        {
                            checkpointCount += await conn.ExecuteAsync(
                                Queries.DB_QUERY_CR_INSERT_CP,
                                checkpointParams,
                                transaction: trx
                            );
                        }
                    }
                });

                // All good
                return CreatedAtAction(nameof(UpdateMapTime), new { id = mapTimeId }, new PostResponseDto
                {
                    Id = mapTimeId,
                    Inserted = checkpointCount > 0 ? checkpointCount + 1 : 1,
                    Trx = checkpointCount > 0
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to update MapTime and Checkpoints for ID {MapTimeID}.", mapTimeId);
                return StatusCode(500, new { error = ex.Message });
            }
        }
    }
}
