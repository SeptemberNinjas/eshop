namespace eshop.Application.Order;

public record BasketDto(int Id, string CustomerName, decimal TotalSum, IEnumerable<BasketItemDto> Items);