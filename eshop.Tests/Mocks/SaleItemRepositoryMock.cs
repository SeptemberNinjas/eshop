using eshop.Core;

namespace eshop.Tests.Mocks
{
    public class SaleItemRepositoryMock : IReadOnlyRepository<SaleItem>
    {
        public Task<IReadOnlyCollection<SaleItem>> GetAllAsync(CancellationToken cancellationToken = default)
        {
            return Task.FromResult((IReadOnlyCollection<SaleItem>)CatalogHelper.Catalog);
        }

        public Task<SaleItem?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(CatalogHelper.Catalog.FirstOrDefault(item => item.Id == id));
        }

        public Task<int> GetCountAsync(CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }
    }
}
