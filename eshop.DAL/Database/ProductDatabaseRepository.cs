using System.Data;
using eshop.Core;
using Npgsql;

namespace eshop.DAL.Database
{
    /// <summary>
    /// Реализация репозитория для хранения товаров в БД
    /// </summary>
    internal class ProductDatabaseRepository : DatabaseContext, IRepositoryAsync<Product>, IRepository<Product>, IRepository<SaleItem>
    {
        public ProductDatabaseRepository(string connectionString) : base(connectionString)
        {
        }

        #region Async

        /// <inheritdoc/>
        public async Task<IReadOnlyCollection<Product>> GetAllAsync(CancellationToken cancellationToken = default)
        {
            using var command = GetCommand(
                @"select c.*, s.amount 
                    from catalog c
                        left join stock s on c.Id = s.Id
                    where type = 1");

            using var reader = await command.ExecuteReaderAsync(cancellationToken);

            var result = new List<Product>();

            while (await reader.ReadAsync(cancellationToken))
            {
                result.Add(GetProduct(reader));
            }

            return result;
        }

        /// <inheritdoc/>
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

        /// <inheritdoc/>
        public async Task<int> GetCountAsync(CancellationToken cancellationToken = default)
        {
            using var command = GetCommand(
                "select count(*) from catalog where type = 1");

            var result = await command.ExecuteScalarAsync(cancellationToken);

            return int.TryParse(result?.ToString(), out var count) ? count : 0;
        }

        public Task UpdateAsync(Product item)
        {
            throw new NotImplementedException();
        }

        public Task<int> InsertAsync(Product item)
        {
            throw new NotImplementedException();
        }

        #endregion

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

        public IReadOnlyCollection<Product> GetAll()
        {
            return Task.Run(async () =>
            {
                return await GetAllAsync();
            }).Result;
        }

        public int GetCount()
        {
            return Task.Run(async () =>
            {
                return await GetCountAsync();
            }).Result;
        }

        public Product? GetById(int id)
        {
            return Task.Run(async () =>
            {
                return await GetByIdAsync(id);
            }).Result;
        }

        IReadOnlyCollection<SaleItem> IReadOnlyRepository<SaleItem>.GetAll()
        {
            return GetAll();
        }

        SaleItem? IReadOnlyRepository<SaleItem>.GetById(int id)
        {
            return GetById(id);
        }
    }
}