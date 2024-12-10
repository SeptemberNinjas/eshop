using eshop.DAL.LinqToDb.Schema;
using LinqToDB;
using LinqToDB.Data;

namespace eshop.DAL.LinqToDb
{
    public class LinqToDbContext : DataConnection
    {
        public ITable<Catalog> Catalog => this.GetTable<Catalog>();

        public ITable<Stock> Stock => this.GetTable<Stock>();

        public LinqToDbContext(DataOptions options) : base(options) { }
    }
}
