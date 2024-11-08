using System.Data;
using System.Data.Common;
using eshop.Core;

namespace eshop.DAL.Database
{
    /// <summary>
    /// Реализация репозитория для хранения остатков в БД
    /// </summary>
    internal class StockDatabaseRepository : DatabaseContext, IRepository<Stock>
    {
        public StockDatabaseRepository(string connectionString) : base(connectionString) { }

        /// <inheritdoc/>
        public IReadOnlyCollection<Stock> GetAll()
        {
            using var command = GetCommand(
                @"select * from stock");

            using var reader = command.ExecuteReader();

            var result = new List<Stock>();

            while (reader.Read())
            {
                result.Add(GetStock(reader));
            }

            return result;
        }

        /// <inheritdoc/>
        public Stock? GetById(int id)
        {
            using var command = GetCommand(
                $@"select s.*
                    from stock s
                    where s.id = {id}");

            using var reader = command.ExecuteReader();

            if (reader.Read())
                return GetStock(reader);

            return null;
        }

        /// <inheritdoc/>
        public int GetCount()
        {
            using var command = GetCommand("select count(*) from stock");
            var result = command.ExecuteScalar();

            return int.TryParse(result?.ToString(), out var count) ? count : 0;
        }

        private static Stock GetStock(DbDataReader reader)
        {
            return new Stock
            {
                ItemId = reader.GetFieldValue<int>("id"),
                Amount = reader.GetFieldValue<int>("amount")
            };
        }

        public void Update(Stock item)
        {
            using var command = GetCommand(
                $"""
                 update stock set
                 amount = {item.Amount}
                 where id = {item.ItemId}
                 """);
            command.ExecuteNonQuery();
        }

        public int Insert(Stock item)
        {
            using var command = GetCommand(
                $"""
                 insert into stock(id, amount) values 
                 ({item.ItemId}, {item.Amount})
                 """);

            var result = command.ExecuteScalar();

            return int.TryParse(result?.ToString(), out var count) ? count : 0;
        }

        public async Task UpdateAsync(Stock item, CancellationToken cancellationToken)
        {
            using var command = GetCommand(
                $"""
                 update stock set
                 amount = {item.Amount}
                 where id = {item.ItemId}
                 """);

            await command.ExecuteNonQueryAsync(cancellationToken);
        }

        public async Task<int> InsertAsync(Stock item, CancellationToken cancellationToken)
        {
            var commandText = 
                $"""
                 insert into stock(id, amount) values 
                 ({item.ItemId}, {item.Amount})
                 """;

            var result = await ExecuteReaderAsync(commandText, (reader) =>
            {
                return int.TryParse(reader[0]?.ToString(), out var count) ? count : 0;
            }, cancellationToken);

            return result;
        }

        public async Task<IReadOnlyCollection<Stock>> GetAllAsync(CancellationToken cancellationToken = default)
        {
            var commandText =
                @"select * from stock";

            var result = await ExecuteReaderListAsync(commandText, GetStock, cancellationToken);

            return result;
        }

        public async Task<int> GetCountAsync(CancellationToken cancellationToken = default)
        {
            var commandText =
                "select count(*) from stock";

            var result = await ExecuteReaderAsync(commandText, (reader) =>
            {
                return int.TryParse(reader[0]?.ToString(), out var count) ? count : 0;
            }, cancellationToken);

            return result;
        }

        public async Task<Stock?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
        {
            var commandText =
                $@"select s.*
                    from stock s
                    where s.id = {id}";

            var result = await ExecuteReaderAsync(commandText, GetStock, cancellationToken);

            return result;
        }
    }
}
