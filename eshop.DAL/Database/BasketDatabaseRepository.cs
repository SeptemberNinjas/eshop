using System.Data;
using eshop.Core;
using Npgsql;

namespace eshop.DAL.Database;

internal class BasketDatabaseRepository : DatabaseContext, IRepository<Basket>
{
    public BasketDatabaseRepository(string connectionString) : base(connectionString)
    {
    }
    
    public IReadOnlyCollection<Basket> GetAll()
    {
        var basket = GetById(default);
        return basket is null ? [] : [basket];
    }

    public int GetCount()
    {
        return GetById(default) is null ? 0 : 1;
    }

    public Basket? GetById(int id)
    {
        using var command = GetCommand(
            $"""
             select c.*, s.*, bl.count 
                 from basket_line bl
                 join catalog c on bl.item_id = c.id
                     left join stock s on c.id = s.id
             """);

        using var reader = command.ExecuteReader();
        if (!reader.HasRows)
            return null;

        var lines = new List<ItemsListLine>();
        
        while (reader.Read())
        {
            lines.Add(GetBasketLine(reader));
        }

        if (lines.Count > 0)
            return new Basket(lines);

        return null;
    }

    public void Update(Basket item)
    {
        Insert(item);
    }

    public int Insert(Basket item)
    {
        using var command = item.Lines.Count > 0
            ? GetCommand(
                $"""
                 truncate basket_line;
                 insert into basket_line(item_id, count) values 
                 {string.Join(',', item.Lines.Select(l => $"({l.ItemId},{l.Count})"))}
                 """)
            : GetCommand(
                $"""
                 truncate basket_line;
                 """);
        return command.ExecuteNonQuery();
    }

    private static ItemsListLine GetBasketLine(NpgsqlDataReader reader)
    {
        var itemType = (ItemTypes)reader.GetFieldValue<int>("type");
        SaleItem item = itemType == ItemTypes.Product
            ? new Product(reader.GetFieldValue<int>("id"),
                reader.GetFieldValue<string>("name"),
                reader.GetFieldValue<decimal>("price"),
                reader.GetFieldValue<int>("amount"))
            : new Service(reader.GetFieldValue<int>("id"),
                reader.GetFieldValue<string>("name"),
                reader.GetFieldValue<decimal>("price"));

        return new ItemsListLine(item, reader.GetFieldValue<int>("count"));
    }

    public Task UpdateAsync(Basket item)
    {
        throw new NotImplementedException();
    }

    public Task<int> InsertAsync(Basket item)
    {
        throw new NotImplementedException();
    }

    public Task<IReadOnlyCollection<Basket>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public Task<int> GetCountAsync(CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public Task<Basket?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }
}