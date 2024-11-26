using eshop.Core;

namespace eshop.DAL.Database;

public record BasketRow(int Id, string Customer, ItemsListLine? Item);
