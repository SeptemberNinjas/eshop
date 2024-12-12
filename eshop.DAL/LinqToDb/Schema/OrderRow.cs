using LinqToDB.Mapping;

namespace eshop.DAL.LinqToDb.Schema;

[Table(Name = "order")]
public class OrderRow
{
    [PrimaryKey, Column("id")]
    public int Id { get; set; }

    [Column("status"), NotNull]
    public int Status { get; set; }
    
    [Association(ThisKey = "Id", OtherKey = "OrderId", CanBeNull = false)]
    public IEnumerable<OrderLine>? Lines { get; set; }
}