using Npgsql;

using System.Data;
using System.Data.Common;

namespace eshop.DAL.Database
{
    /// <summary>
    /// Контекст подключения к СУБД
    /// </summary>
    public class DatabaseContext : IDisposable
    {
        private readonly string _connectionString;

        private NpgsqlConnection? _connection;

        private NpgsqlTransaction? _transaction;

        public DatabaseContext(string connectionString)
        {
            _connectionString = connectionString;
        }

        /// <inheritdoc/>
        public void Dispose()
        {
            _connection?.Dispose();
        }

        private async Task<NpgsqlConnection> GetConnectionAsync()
        {
            if (_connection != null && _connection.State == ConnectionState.Open)
                return _connection;

            _connection = new NpgsqlConnection(_connectionString);

            await _connection.OpenAsync();

            return _connection;
        } 

        public async Task BeginTransactionAsync()
        {
            var connection = await GetConnectionAsync();

            _transaction = await connection.BeginTransactionAsync();
        }

        public async Task CommitTransactionAsync()
        {
            if (_transaction != null)
                await _transaction.CommitAsync();
        }

        /// <summary>
        /// Получить команду для СУБД
        /// </summary>
        public NpgsqlCommand GetCommand(string text)
        {
            return Task.Run(async () => await GetCommandAsync(text)).Result;
        }

        public async Task<NpgsqlCommand> GetCommandAsync(string text)
        {
            return new NpgsqlCommand
            {
                Connection = await GetConnectionAsync(),
                CommandType = CommandType.Text,
                CommandText = text
            };
        }

        

        
    }
}
