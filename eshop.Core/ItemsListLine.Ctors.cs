namespace eshop.Core
{
    public class ItemsListLine : ItemsListLine<SaleItem>
    {
        public ItemsListLine(Product product, int requestedCount) : base(product, requestedCount) { }

        public ItemsListLine(Service service) : base(service, 1) { }
    }
}
