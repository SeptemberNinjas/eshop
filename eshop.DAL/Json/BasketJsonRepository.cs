using eshop.Core;

namespace eshop.DAL.Json;

/// <summary>
/// Json репозиторий для работы с корзиной
/// </summary>
internal class BasketJsonRepository : JsonRepository<Basket>, IRepository<Basket>
{
    /// <inheritdoc />
    private protected override string ResourceFilePath => "data\\basket.json";

    /// <inheritdoc />
    public IReadOnlyCollection<Basket> GetAll() => (IReadOnlyCollection<Basket>)GetItemsFromFile();

    /// <inheritdoc />
    public int GetCount() => 1;

    /// <inheritdoc />
    public Basket? GetById(int id) => GetAll().FirstOrDefault();

    /// <inheritdoc />
    public void Update(Basket item)
    {
       SaveItemsToFile([item]);
    }

    public int Insert(Basket item)
    {
        throw new NotSupportedException("Нельзя создать вторую корзину");
    }
}