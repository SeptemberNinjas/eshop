using Npgsql;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace eshop.DAL
{
    internal class DatabaseContext : IDisposable
    {
        private readonly string _connectionString;

        private NpgsqlConnection? _connection;

        public DatabaseContext(string connectionString)
        {
            _connectionString = connectionString;
        }

        public void Dispose()
        {
            _connection?.Dispose();
        }

        public NpgsqlConnection GetConnection()
        {
            if (_connection != null && _connection.State == ConnectionState.Open)
                return _connection;

            _connection = new NpgsqlConnection(_connectionString);

            _connection.Open();

            return _connection;
        }

        public NpgsqlCommand GetCommand(string text)
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
