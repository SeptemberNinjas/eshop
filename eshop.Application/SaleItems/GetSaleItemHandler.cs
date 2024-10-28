using eshop.Core;
using eshop.DAL;

namespace eshop.Application.SaleItems;

public class GetSaleItemHandler
{
    private readonly RepositoryFactory _repositoryFactory;

    public GetSaleItemHandler(RepositoryFactory repositoryFactory)
    {
        _repositoryFactory = repositoryFactory;
    }

    public async Task<IEnumerable<SaleItemDto>> GetItemsAsync(ItemTypes itemType, int? count)
    {
        var repository = _repositoryFactory.CreateSaleItemRepository();
        var items = repository
            .GetAll()
            .Where(i => i.ItemType == itemType);
        var requestedItems = count is null or <= 0
            ? items
            : items.Take(count.Value);
        
        return requestedItems
            .Select(i => new SaleItemDto(i.ItemType, i.Id, i.Name, i.Price, (i as Product)?.Stock));
    }
}