using LinqToDB.Mapping;

namespace eshop.DAL.LinqToDb.Schema;

[Table(Name = "customer_orders")]
public class CustomerOrders
{
    [Column("customer", Length = 128), NotNull]
    public string? Customer { get; set; }
    
    [Column("order_id"), NotNull]
    public int OrderId { get; set; }
    
    [Association(ThisKey = "OrderId", OtherKey = "Id", CanBeNull = false)]
    public IEnumerable<OrderRow>? Orders { get; set; }
}