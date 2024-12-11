using LinqToDB.Mapping;

namespace eshop.DAL.LinqToDb.Schema;

[Table(Name = "order_line")]
public class OrderLine
{
    [Column("order_id"), NotNull]
    public int OrderId { get; set; }

    [Column("item_id"), NotNull]
    public int ItemId { get; set; }
    
    [Column("count"), NotNull]
    public int Count { get; set; }
    
    [Association(ThisKey = "ItemId", OtherKey = "Id", CanBeNull = false)]
    public Catalog? Item { get; set; }
}