using System.Data;
using System.Data.Common;
using eshop.Core;

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
            var commandText = 
                @"select c.*, s.amount 
                    from catalog c
                        left join stock s on c.Id = s.Id
                    where type = 1";

            var result = await ExecuteReaderListAsync(commandText, GetProduct, cancellationToken);

            return result;
        }

        /// <inheritdoc/>
        public async Task<Product?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
        {
            var commandText =
                $@"select c.*, s.amount 
                    from catalog c
                        left join stock s on c.Id = s.Id
                    where type = 1 and c.id = {id}";

            var result = await ExecuteReaderAsync(commandText, GetProduct, cancellationToken);

            return result;
        }

        /// <inheritdoc/>
        public async Task<int> GetCountAsync(CancellationToken cancellationToken = default)
        {
            var commandText=
                "select count(*) from catalog where type = 1";

            var result = await ExecuteReaderAsync(commandText, (reader) =>
            {
                return int.TryParse(reader[0]?.ToString(), out var count) ? count : 0;
            }, cancellationToken);

            return result;
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