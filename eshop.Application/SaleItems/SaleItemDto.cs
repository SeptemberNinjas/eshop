using eshop.Core;

namespace eshop.Application.SaleItems;

public record SaleItemDto(ItemTypes ItemType, int Id, string Name, decimal Price, decimal? Stock = null);
