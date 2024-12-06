using LinqToDB.Mapping;

namespace eshop.DAL.LinqToDb.Schema
{
    [Table(Name = "stock")]
    public class Stock
    {
        [Column("id")]
        public int Id { get; set; }

        [Column("amount")]
        public int Amount { get; set; }
    }
}
