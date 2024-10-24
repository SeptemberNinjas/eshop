using System.Data;
using System.Data.Common;
using eshop.Core;
using Npgsql;

namespace eshop.DAL.Database
{
    /// <summary>
    /// Реализация репозитория для хранения товаров в БД
    /// </summary>
    internal class ProductDatabaseRepository : DatabaseContext, IRepository<Product>
    {
        public ProductDatabaseRepository(string connectionString) : base(connectionString)
        {
        }

        /// <inheritdoc/>
        public IReadOnlyCollection<Product> GetAll()
        {
            return GetAllAsync().Result;
    }

        /// <inheritdoc/>
        public Product? GetById(int id)
        {
            return GetByIdAsync(id).Result;
        }

        /// <inheritdoc/>
        public int GetCount()
        {
            return GetCountAsync().Result;
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

        private static Product GetProduct(DbDataReader reader)
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

        public Task UpdateAsync(Product item)
        {
            throw new NotImplementedException();
        }

        public Task<int> InsertAsync(Product item)
        {
            throw new NotImplementedException();
        }

        public Task<IReadOnlyCollection<Product>> GetAllAsync(
            CancellationToken cancellationToken = default)
        {
            var command = @"select c.*, s.amount 
                from catalog c
                    left join stock s on c.Id = s.Id
                where type = 1";

            return ExecuteReaderListAsync<Product>(command, cancellationToken, 
                GetProduct);
        }

        public async Task<int> GetCountAsync(CancellationToken cancellationToken = default)
        {
            using var command = GetCommand(
             "select count(*) from catalog where type = 1");

            var result = await command.ExecuteScalarAsync(cancellationToken);

            return int.TryParse(result?.ToString(), out var count) ? count : 0;
        }

        public async Task<Product?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
        {
            using var command = GetCommand(
           $@"select c.*, s.amount 
                from catalog c
                    left join stock s on c.Id = s.Id
                where type = 1 and c.id = {id}");

            using var reader = await command.ExecuteReaderAsync(cancellationToken);

            if (await reader.ReadAsync(cancellationToken))
                return GetProduct(reader);

            return null;
        }



    }
}