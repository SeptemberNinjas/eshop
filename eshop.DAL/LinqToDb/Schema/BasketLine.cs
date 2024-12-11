using LinqToDB.Mapping;

namespace eshop.DAL.LinqToDb.Schema;

[Table(Name = "basket_line")]
public class BasketLine
{
    [Column("basket_id"), NotNull]
    public int BasketId { get; set; }

    [Column("item_id"), NotNull]
    public int ItemId { get; set; }
    
    [Column("count"), NotNull]
    public int Count { get; set; }
    
    [Association(ThisKey = "ItemId", OtherKey = "Id", CanBeNull = false)]
    public Catalog? Item { get; set; }
}