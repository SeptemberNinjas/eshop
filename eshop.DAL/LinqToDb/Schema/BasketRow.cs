using LinqToDB.Mapping;

namespace eshop.DAL.LinqToDb.Schema;

[Table(Name = "basket")]
public class BasketRow
{
    [PrimaryKey, Column("id")]
    public int Id { get; set; }

    [Column("customer", Length = 128), NotNull]
    public string? Customer { get; set; }
    
    [Association(ThisKey = "Id", OtherKey = "BasketId", CanBeNull = true)]
    public IEnumerable<BasketLine>? Lines { get; set; }
}