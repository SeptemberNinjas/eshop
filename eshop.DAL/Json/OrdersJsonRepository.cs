using eshop.Core;

namespace eshop.DAL.Json;

internal class OrdersJsonRepository : JsonRepository<OrderEntity>, IRepository<Order>
{
    /// <inheritdoc />
    private protected override string ResourceFilePath => "data\\orders.json";

    /// <inheritdoc />
    public IReadOnlyCollection<Order> GetAll() => GetItemsFromFile()
        .Select(o => new Order(o.Id, o.Status, 
            o.Lines.Select(l => l.ItemType == ItemTypes.Product 
                ? new ItemsListLine(l.Product!, l.Count) 
                : new ItemsListLine(l.Service!))))
        .ToArray();

    /// <inheritdoc />
    public int GetCount() => GetAll().Count;

    /// <inheritdoc />
    public Order? GetById(int id) => GetAll().FirstOrDefault(o => o.Id == id);
    
    /// <inheritdoc />
    public void Update(Order item)
    {
        var updatedList = GetItemsFromFile() // Подменим в коллекции объект на тот который пришел аргументом
            .Select(o => o.Id == item.Id ? (OrderEntity)item : o); 
        
        SaveItemsToFile(updatedList);
    }

    /// <inheritdoc />
    public int Insert(Order item)
    {
        var orders = GetItemsFromFile().ToList();
        var lastId = orders.Count > 0 ? orders.Max(o => o.Id) : 0;
        item.SetNewOrderId(++lastId);
        orders.Add((OrderEntity)item);
        SaveItemsToFile(orders);

        return lastId;
    }

    public Task UpdateAsync(Order item)
    {
        throw new NotImplementedException();
    }

    public Task<int> InsertAsync(Order item)
    {
        throw new NotImplementedException();
    }

    public Task<IReadOnlyCollection<Order>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public Task<int> GetCountAsync(CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public Task<Order?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }
}