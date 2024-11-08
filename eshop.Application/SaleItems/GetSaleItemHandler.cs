using eshop.Core;
using eshop.DAL;
using FluentResults;

namespace eshop.Application.SaleItems
{
    public class GetSaleItemHandler
    {
        private readonly RepositoryFactory _repositoryFactory;

        public GetSaleItemHandler(RepositoryFactory repositoryFactory)
        {
            _repositoryFactory = repositoryFactory;
        }

        public async Task<Result<IEnumerable<SaleItemDto>>> GetItemsAsync(ItemTypes itemType, int? count)
        {
            var repository = _repositoryFactory.CreateSaleItemRepository();

            try
            {
                var items = (await repository
                    .GetAllAsync())
                    .Where(i => i.ItemType == itemType);

                var requestedItems = count is null or <= 0
                    ? items
                    : items.Take(count.Value);

                return Result.Ok(requestedItems
                    .Select(i => new SaleItemDto(i.ItemType, i.Id, i.Name, i.Price, (i as Product)?.Stock)));
            }
            catch (Exception ex)
            {
                return Result.Fail("Не удалось получить коллекцию торговых единиц")
                    .WithError(ex.Message)
                    .WithError(ex.StackTrace);
            }
        }
    }
}