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
    }
}
