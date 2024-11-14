using System.Data;
using System.Data.Common;
using eshop.Core;

namespace eshop.DAL.Database
{
    /// <summary>
    /// Реализация репозитория для хранения товаров в БД
    /// </summary>
    internal class SaleItemDatabaseRepository : DatabaseContext, IReadOnlyRepository<SaleItem>
    {
        public SaleItemDatabaseRepository(string connectionString) : base(connectionString)
        {
        }

        private static SaleItem GetSaleItem(DbDataReader reader)
        {
            var type = (ItemTypes)reader.GetFieldValue<int>("type");
            var id = reader.GetFieldValue<int>("id");
            var name = reader.GetFieldValue<string>("name");
            var price = reader.GetFieldValue<decimal>("price");
            
            return type switch
            {
                ItemTypes.Product => new Product(id, name, price,
                    reader.GetFieldValue<int>("amount")),
                ItemTypes.Service => new Service(id, name, price),
                _ => throw new ArgumentOutOfRangeException()
            };
        }

        /// <inheritdoc/>
        public async Task<IReadOnlyCollection<SaleItem>> GetAllAsync(CancellationToken cancellationToken = default)
        {
            var commandText =
                @"select c.*, s.amount 
                    from catalog c
                        left join stock s on c.Id = s.Id";

            var result = await ExecuteReaderListAsync(commandText, GetSaleItem, cancellationToken);

            return result;
        }

        /// <inheritdoc/>
        public async Task<int> GetCountAsync(CancellationToken cancellationToken = default)
        {
            var commandText = 
                "select count(*) from catalog";

            var result = await ExecuteReaderAsync(commandText, 
                reader => int.TryParse(reader[0].ToString(), out var count) ? count : 0, cancellationToken);

            return result;
        }

        /// <inheritdoc/>
        public async Task<SaleItem?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
        {
            var commandText =
                $@"select c.*, s.amount 
                    from catalog c
                        left join stock s on c.Id = s.Id
                    where c.id = {id}";

            var result = await ExecuteReaderAsync(commandText, GetSaleItem, cancellationToken);

            return result;
        }
    }
}