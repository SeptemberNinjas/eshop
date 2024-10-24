using Npgsql;

using System.Data;
using System.Data.Common;

namespace eshop.DAL.Database
{
    /// <summary>
    /// Контекст подключения к СУБД
    /// </summary>
    internal class DatabaseContext : IDisposable
    {
        private readonly string _connectionString;

        private NpgsqlConnection? _connection;

        public DatabaseContext(string connectionString)
        {
            _connectionString = connectionString;
        }

        /// <inheritdoc/>
        public void Dispose()
        {
            _connection?.Dispose();
        }

        /// <summary>
        /// Получить соединение с БД
        /// </summary>
        private NpgsqlConnection GetConnection()
        {
            if (_connection != null && _connection.State == ConnectionState.Open)
                return _connection;

            _connection = new NpgsqlConnection(_connectionString);
            
            _connection.Open();

            return _connection;
        }

        /// <summary>
        /// Получить команду для СУБД
        /// </summary>
        private protected NpgsqlCommand GetCommand(string text)
        {
            return new NpgsqlCommand
            {
                Connection = GetConnection(),
                CommandType = CommandType.Text,
                CommandText = text
            };
        }

        private async Task<NpgsqlConnection> GetConnectionAsync()
        {
            if (_connection != null && _connection.State == ConnectionState.Open)
                return _connection;

            _connection = new NpgsqlConnection(_connectionString);

            await _connection.OpenAsync();

            return _connection;
        }

        protected async Task<IReadOnlyCollection<T>> ExecuteReaderListAsync<T>(string commandText,
            CancellationToken cancellationToken, Func<DbDataReader, T> binding)
        {
            using var connection = await GetConnectionAsync();

            var command = GetCommand(commandText);

            using var reader = await command.ExecuteReaderAsync(cancellationToken);

            var result = new List<T>();

            while (await reader.ReadAsync(cancellationToken))
            {
                result.Add(binding(reader));
            }

            return result;
        }
    }
}
