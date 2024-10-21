using eshop.Core;
using Npgsql;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace eshop.DAL.DB
{
    internal class ProductDatabaseRepository : DatabaseContext, IRepository<Product>
    {
        public ProductDatabaseRepository(string connectionString) : base(connectionString) { }

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

        public int GetCount()
        {
            using var command = GetCommand(
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

        private static Product GetProduct(NpgsqlDataReader reader)
        {
            return new Product(
                    reader.GetFieldValue<int>("id"),
                    reader.GetFieldValue<string>("name"),
                    reader.GetFieldValue<decimal>("price"),
                    reader.GetFieldValue<int>("amount"));
        }
    }
}
