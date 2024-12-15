using eshop.Core;

namespace eshop.Application.Order;

public record OrderItemDto(int Id, ItemTypes ItemType, string Name, int Amount, decimal Price, decimal Sum);