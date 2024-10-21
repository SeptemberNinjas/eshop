using eshop.Core;

using Npgsql;

using System.Data;

namespace eshop.DAL.Database
{
    /// <summary>
    /// Реализация репозитория для хранения товаров в БД
    /// </summary>
    internal class ProductDatabaseRepository : DatabaseContext, IRepository<Product>
    {
        public ProductDatabaseRepository(string connectionString) : base(connectionString) { }
        
        /// <inheritdoc/>
        public IReadOnlyCollection<Product> GetAll()
        {
            using var connection = GetConnection();

            using var command = GetCommand(connection,
                @"select c.*, s.amount 
                    from catalog c
                        left join stock s on c.Id = s.Id
                    where type = 1");

            using var reader = command.ExecuteReader();

            var result = new List<Product>();

            while (reader.Read())
            {
                result.Add(GetProduct(reader));
            }

            return result;
        }

        /// <inheritdoc/>
        public Product? GetById(int id)
        {
            using var connection = GetConnection();

            using var command = GetCommand(connection,
                $@"select c.*, s.amount 
                    from catalog c
                        left join stock s on c.Id = s.Id
                    where type = 1 and c.id = {id}");

            using var reader = command.ExecuteReader();

            if (reader.Read())
                return GetProduct(reader);

            return null;
        }

        /// <inheritdoc/>
        public int GetCount()
        {
            using var connection = GetConnection();

            using var command = GetCommand(connection,
                "select count(*) from catalog where type = 1");

            var result = command.ExecuteScalar();

            if (int.TryParse(result?.ToString(), out int count))
                return count;
            else
                return 0;
        }

        public int Insert(Product item)
        {
            throw new NotImplementedException();
        }

        public void Update(Product item)
        {
            throw new NotImplementedException();
        }

        private Product GetProduct(NpgsqlDataReader reader)
        {
            return new Product(
                    reader.GetFieldValue<int>("id"),
                    reader.GetFieldValue<string>("name"),
                    reader.GetFieldValue<decimal>("price"),
                    reader.GetFieldValue<int>("amount"));
        }
    }
}
