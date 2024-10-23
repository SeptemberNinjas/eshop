using System.Data;
using eshop.Core;
using Npgsql;

namespace eshop.DAL.Database
{
    /// <summary>
    /// Реализация репозитория для хранения товаров в БД
    /// </summary>
    internal class ProductDatabaseRepository : DatabaseContext, IRepository<Product>, IRepository<SaleItem>
    {
        public ProductDatabaseRepository(string connectionString) : base(connectionString)
        {
        }

        /// <inheritdoc/>
        public IReadOnlyCollection<Product> GetAll()
        {
            using var command = GetCommand(
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
            using var command = GetCommand(
                $@"select c.*, s.amount 
                    from catalog c
                        left join stock s on c.Id = s.Id
                    where type = 1 and c.id = {id}");

            using var reader = command.ExecuteReader();

            if (reader.Read())
                return GetProduct(reader);

            return null;
        }

        IReadOnlyCollection<SaleItem> IReadOnlyRepository<SaleItem>.GetAll()
        {
            return GetAll();
        }

        /// <inheritdoc/>
        public int GetCount()
        {
            using var command = GetCommand(
                "select count(*) from catalog where type = 1");

            var result = command.ExecuteScalar();

            return int.TryParse(result?.ToString(), out var count) ? count : 0;
        }

        SaleItem? IReadOnlyRepository<SaleItem>.GetById(int id)
        {
            return GetById(id);
        }

        public int Insert(Product item)
        {
            throw new NotImplementedException();
        }

        public void Update(Product item)
        {
            // Обновляем только остатки
            using var command = GetCommand(
                $"""
                 update stock set
                    amount = {item.Stock}
                    where id = {item.Id}
                 """);

            command.ExecuteNonQuery();
        }

        private static Product GetProduct(NpgsqlDataReader reader)
        {
            return new Product(
                reader.GetFieldValue<int>("id"),
                reader.GetFieldValue<string>("name"),
                reader.GetFieldValue<decimal>("price"),
                reader.GetFieldValue<int>("amount"));
        }

        public void Update(SaleItem item)
        {
            if (item is Product product)
                Update(product);
        }

        public int Insert(SaleItem item)
        {
            if (item is Product product)
                return Insert(product);
            
            throw new ApplicationException("Неверный репозиторий");
        }
    }
}