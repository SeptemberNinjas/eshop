using LinqToDB.Mapping;

namespace eshop.DAL.LinqToDb.Schema
{
    [Table(Name = "catalog")]
    public class Catalog
    {
        [Column("id")]
        public int Id { get; set; }

        [Column("name")]
        public string Name { get; set; } = null!;

        [Column("price")]
        public decimal Price { get; set; }

        [Column("type")]
        public short Type { get; set; }
    }
}
