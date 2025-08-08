using Dapper;
using MySqlConnector;
using System.Data;

namespace CS2_Surf_NET_API.Data
{
    public class DatabaseService : IDatabaseService
    {
        private readonly MySqlDataSource _dataSource;

        public DatabaseService(MySqlDataSource dataSource)
        {
            _dataSource = dataSource;
        }

        public async Task<IEnumerable<T>> QueryAsync<T>(string sql, object? parameters = null)
        {
            await using var connection = await _dataSource.OpenConnectionAsync();


            ///var rows = await connection.QueryAsync(sql, parameters);
            ///foreach (var row in rows)
            ///{
            ///    var dict = (IDictionary<string, object>)row;
            ///    foreach (var kvp in dict)
            ///    {
            ///        Console.WriteLine($"{kvp.Key} = {kvp.Value}");
            ///    }
            ///}

            return await connection.QueryAsync<T>(sql, parameters);
        }

        public async Task<T?> QueryFirstOrDefaultAsync<T>(string sql, object? parameters = null)
        {
            await using var connection = await _dataSource.OpenConnectionAsync();
            return await connection.QueryFirstOrDefaultAsync<T>(sql, parameters);
        }

        public async Task<long> ExecuteAsync(string sql, object? parameters = null)
        {
            await using var connection = await _dataSource.OpenConnectionAsync();
            await using var transaction = await connection.BeginTransactionAsync();

            try
            {
                _ = await connection.ExecuteAsync(sql, parameters, transaction);

                // Check if the last query was INSERT in order to return LAST_INSERT_ID()
                if (sql.TrimStart().StartsWith("INSERT", StringComparison.OrdinalIgnoreCase))
                {
                    var insertedId = await connection.ExecuteScalarAsync<long>("SELECT LAST_INSERT_ID();", transaction: transaction);
                    await transaction.CommitAsync();
                    return insertedId;
                }

                await transaction.CommitAsync();
                return 0;
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        public async Task TransactionAsync(Func<IDbConnection, IDbTransaction, Task> operations)
        {
            await using var connection = await _dataSource.OpenConnectionAsync();
            using var transaction = await connection.BeginTransactionAsync();

            try
            {
                await operations(connection, transaction);
                await transaction.CommitAsync();
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }
    }
}
