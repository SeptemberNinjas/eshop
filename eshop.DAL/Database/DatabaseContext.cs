using Npgsql;

using System.Data;

namespace eshop.DAL.Database
{
    /// <summary>
    /// Контекст подключения к СУБД
    /// </summary>
    public class DatabaseContext : IDisposable
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
    }
}
