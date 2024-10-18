using eshop.Core;

namespace eshop.DAL.Json;

internal class OrdersJsonRepository : JsonRepository<Order>, IRepository<Order>
{
    /// <inheritdoc />
    private protected override string ResourceFilePath => "data\\orders.json";

    /// <inheritdoc />
    public IReadOnlyCollection<Order> GetAll() => (IReadOnlyCollection<Order>)GetItemsFromFile();

    /// <inheritdoc />
    public int GetCount() => GetAll().Count;

    /// <inheritdoc />
    public Order? GetById(int id) => GetAll().FirstOrDefault(o => o.Id == id);
    
    /// <inheritdoc />
    public void Update(Order item)
    {
        var updatedList = GetAll()
            .Select(o => o.Id == item.Id ? item : o); // Подменим в коллекции объект на тот который пришел аргументом
        
        SaveItemsToFile(updatedList);
    }

    /// <inheritdoc />
    public int Insert(Order item)
    {
        var orders = GetItemsFromFile().ToList();
        var lastId = orders.Max(o => o.Id);
        item.SetNewOrderId(++lastId);
        orders.Add(item);
        SaveItemsToFile(orders);

        return lastId;
    }
}