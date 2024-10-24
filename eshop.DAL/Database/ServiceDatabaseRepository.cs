using System.Data;
using eshop.Core;
using Npgsql;

namespace eshop.DAL.Database
{
    /// <summary>
    /// Реализация репозитория для хранения услуг в БД
    /// </summary>
    internal class ServiceDatabaseRepository : DatabaseContext, IReadOnlyRepository<Service>
    {
        public ServiceDatabaseRepository(string connectionString) : base(connectionString) { }

        /// <inheritdoc/>
        public IReadOnlyCollection<Service> GetAll()
        {
            using var command = GetCommand(
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
            using var command = GetCommand(
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
            using var command = GetCommand(
                "select count(*) from catalog where type = 2");

            var result = command.ExecuteScalar();

            if (int.TryParse(result?.ToString(), out int count))
                return count;
            else
                return 0;
        }

          private static Service GetService(NpgsqlDataReader reader)
        {
            return new Service(
                    reader.GetFieldValue<int>("id"),
                    reader.GetFieldValue<string>("name"),
                    reader.GetFieldValue<decimal>("price"));
        }

        public Task<IReadOnlyCollection<Service>> GetAllAsync(CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<int> GetCountAsync(CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<Service?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }
    }
}
