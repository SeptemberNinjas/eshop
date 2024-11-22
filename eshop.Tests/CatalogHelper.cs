using eshop.Core;

namespace eshop.Tests
{
    public static class CatalogHelper
    {
        public static IEnumerable<SaleItem> Catalog => [
                new Product(1, "Валик", 19.99M, 3),
                new Product(2, "Краскопуль", 200M, 4),
                new Service(3, "Побелить стену", 54.99M),
                new Service(4, "Покрастить дом", 500M)
            ];

        public static IEnumerable<Stock> Stocks
        {
            get
            {
                return Catalog
                        .Where(item => item.ItemType == ItemTypes.Product)
                        .Select(item => new Stock
                        {
                            ItemId = item.Id,
                            Amount = ((Product)item).Stock
                        });
            }
        }
    }
}
