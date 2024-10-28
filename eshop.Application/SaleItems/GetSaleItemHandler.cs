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
        switch (itemType)
        {
            case ItemTypes.Product:
                var productsRepo = _repositoryFactory.CreateProductRepository();
                var products = productsRepo.GetAll();
                return products.Select(i => new SaleItemDto(i.ItemType, i.Id, i.Name, i.Price, i.Stock));
            case ItemTypes.Service:
                var servicesRepo = _repositoryFactory.CreateServiceRepository();
                var services = servicesRepo.GetAll();
                return services.Select(i => new SaleItemDto(i.ItemType, i.Id, i.Name, i.Price));
            default:
                throw new ArgumentOutOfRangeException(nameof(itemType), itemType, null);
        }
        // После подтягивания 6-7 урока надо подтянуть от туда репозитории
        
    }
}