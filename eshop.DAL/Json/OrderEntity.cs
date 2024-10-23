using eshop.Core;

namespace eshop.DAL.Json;

public class OrderEntity
{
    public int Id { get; init; }
    public OrderStatus Status { get; init; }
    public IEnumerable<ItemsListLineEntity> Lines { get; init; }

    public static implicit operator Order (OrderEntity entity)
    {
        return new Order(entity.Id, entity.Status, entity.Lines
            .Select(l => (ItemsListLine)l));
    }
    
    public static implicit operator OrderEntity (Order order)
    {
        return new OrderEntity
        {
            Id = order.Id,
            Status = order.Status,
            Lines = order.Lines.Select(l => (ItemsListLineEntity)l)
        };
    }
}
