using eshop.DAL.LinqToDb.Schema;
using LinqToDB;
using LinqToDB.Data;

namespace eshop.DAL.LinqToDb
{
    public class LinqToDbContext : DataConnection
    {
        public ITable<Catalog> Catalog => this.GetTable<Catalog>();

        public ITable<Stock> Stock => this.GetTable<Stock>();
        
        public ITable<BasketRow> Baskets => this.GetTable<BasketRow>();
        
        public ITable<BasketLine> BasketLines => this.GetTable<BasketLine>();
        
        public ITable<OrderRow> Orders => this.GetTable<OrderRow>();
        
        public ITable<OrderLine> OrderLines => this.GetTable<OrderLine>();

        public LinqToDbContext(DataOptions options) : base(options) { }
    }
}
