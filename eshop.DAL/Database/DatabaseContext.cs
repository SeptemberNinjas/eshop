using Npgsql;

using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
        /// <returns></returns>
        public NpgsqlConnection GetConnection()
        {
            _connection = new NpgsqlConnection(_connectionString);

            _connection.Open();

            return _connection;
        }

        /// <summary>
        /// Получить команду для СУБД
        /// </summary>
        /// <param name="connection"></param>
        /// <param name="text"></param>
        /// <returns></returns>
        public NpgsqlCommand GetCommand(NpgsqlConnection connection, string text)
        {
            return new NpgsqlCommand
            {
                Connection = connection,
                CommandType = CommandType.Text,
                CommandText = text
            };
        }
    }
}
