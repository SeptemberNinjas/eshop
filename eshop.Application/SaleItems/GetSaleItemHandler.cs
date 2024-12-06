using eshop.Core;
using eshop.DAL;
using FluentResults;

namespace eshop.Application.SaleItems
{
    public class GetSaleItemHandler
    {
        private readonly IReadOnlyRepository<SaleItem> _saleItemRepository;

        public GetSaleItemHandler(IReadOnlyRepository<SaleItem> saleItemRepository)
        {
            _saleItemRepository = saleItemRepository;
        }

        public async Task<Result<IEnumerable<SaleItemDto>>> GetItemsAsync(ItemTypes itemType, int? count, CancellationToken cancellationToken)
        {
            try
            {
                var items = (await _saleItemRepository
                    .GetAllAsync(cancellationToken))
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