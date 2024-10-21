using eshop.Core;

using Npgsql;

using System.Data;

namespace eshop.DAL.Database
{
    /// <summary>
    /// Реализация репозитория для хранения услуг в БД
    /// </summary>
    internal class ServiceDatabaseRepository : DatabaseContext, IRepository<Service>
    {
        public ServiceDatabaseRepository(string connectionString) : base(connectionString) { }

        /// <inheritdoc/>
        public IReadOnlyCollection<Service> GetAll()
        {
            using var connection = GetConnection();

            using var command = GetCommand(connection,
                @"select c.*
                    from catalog c
                    where type = 2");

            using var reader = command.ExecuteReader();

            var result = new List<Service>();

            while (reader.Read())
            {
                result.Add(GetService(reader));
            }

            return result;
        }

        /// <inheritdoc/>
        public Service? GetById(int id)
        {
            using var connection = GetConnection();

            using var command = GetCommand(connection,
                $@"select c.*
                    from catalog c
                    where type = 2 and c.id = {id}");

            using var reader = command.ExecuteReader();

            if (reader.Read())
                return GetService(reader);

            return null;
        }

        /// <inheritdoc/>
        public int GetCount()
        {
            using var connection = GetConnection();

            using var command = GetCommand(connection,
                "select count(*) from catalog where type = 2");

            var result = command.ExecuteScalar();

            if (int.TryParse(result?.ToString(), out int count))
                return count;
            else
                return 0;
        }

        public int Insert(Service item)
        {
            throw new NotImplementedException();
        }

        public void Update(Service item)
        {
            throw new NotImplementedException();
        }

        private Service GetService(NpgsqlDataReader reader)
        {
            return new Service(
                    reader.GetFieldValue<int>("id"),
                    reader.GetFieldValue<string>("name"),
                    reader.GetFieldValue<decimal>("price"));
        }
    }
}
