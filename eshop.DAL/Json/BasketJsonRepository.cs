using eshop.Core;

namespace eshop.DAL.Json;

/// <summary>
/// Json репозиторий для работы с корзиной
/// </summary>
internal class BasketJsonRepository : JsonRepository<BasketEntity>, IRepository<Basket>
{
    /// <inheritdoc />
    private protected override string ResourceFilePath => "data\\basket.json";

    /// <inheritdoc />
    public IReadOnlyCollection<Basket> GetAll() => GetItemsFromFile()
        .Select(b => (Basket)b)
        .ToArray();

    /// <inheritdoc />
    public int GetCount() => 1;

    /// <inheritdoc />
    public Basket? GetById(int id) => GetAll().FirstOrDefault();

    /// <inheritdoc />
    public void Update(Basket item)
    {
       SaveItemsToFile([(BasketEntity)item]);
    }

    public int Insert(Basket item)
    {
        throw new NotSupportedException("Нельзя создать вторую корзину");
    }
}