using eshop.Core;

namespace eshop.Application.Order;

public record OrderDto(int Id, OrderStatus Status, decimal TotalSum, IEnumerable<OrderItemDto> Items);