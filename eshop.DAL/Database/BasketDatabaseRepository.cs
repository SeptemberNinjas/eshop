using System.Data;
using System.Data.Common;
using eshop.Core;
using Prometheus;

namespace eshop.DAL.Database;

internal class BasketDatabaseRepository : DatabaseContext, IRepository<Basket>
{
    private readonly Gauge _notEmptyBaskets; 
    
    public BasketDatabaseRepository(string connectionString) : base(connectionString)
    {
        _notEmptyBaskets = Metrics.CreateGauge("eshop_not_empty_baskets", "Количество не пустых корзин");
    }

    public async Task UpdateAsync(Basket item, CancellationToken cancellationToken)
    {
        await using var command = item.Lines.Count > 0
            ? GetCommand(
                $"""
                  delete from basket_line where basket_id = {item.Id};
                  insert into basket_line(basket_id, item_id, count) 
                  values {string.Join(',', item.Lines.Select(l => $"({item.Id},{l.ItemId},{l.Count})"))}
                  """)
            : GetCommand(
                $"""
                 delete from basket_line where basket_id = {item.Id};
                 """);
        
        await command.ExecuteNonQueryAsync(cancellationToken);
    }

    public async Task<int> InsertAsync(Basket item, CancellationToken cancellationToken)
    {
        await using var command = GetCommand(
            $"""
             insert into basket(customer) values ('{item.Customer}')
             """);
        return await command.ExecuteNonQueryAsync(cancellationToken);
    }

    public async Task<IReadOnlyCollection<Basket>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var commandText = 
            $"""
             select b.id as basket_id, b.customer, c.id as item_id, c.price, c."name", c."type" , s.amount , bl.count
             from basket b
             left join basket_line bl on b.id = bl.basket_id
             left join catalog c on bl.item_id = c.id
             left join stock s on c.id = s.id
             """;

        var result = await ExecuteReaderListAsync(commandText, GetBasketLine, cancellationToken);
        var baskets = result
            .GroupBy(r => r.Id)
            .Select(g => new Basket(g.Key, g
                .Where(i => i.Item is not null)
                .Select(i => i.Item!), g.First().Customer))
            .ToArray();
        
        _notEmptyBaskets.Set(baskets.Count(b => b.Lines.Count != 0));

        return baskets;
    }

    public async Task<int> GetCountAsync(CancellationToken cancellationToken = default)
    {
        var commandText = 
            $"""
             select count(*) 
                 from basket b
             """;
        
        var result =  await ExecuteScalarAsync(commandText, cancellationToken);

        return int.TryParse(result, out var count) ? count : default;
    }

    public async Task<Basket?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        var commandText = 
            $"""
             select b.id as basket_id, b.customer, c.id as item_id, c.price, c."name", c."type" , s.amount , bl.count
             from basket b
             left join basket_line bl on b.id = bl.basket_id
             left join catalog c on bl.item_id = c.id
             left join stock s on c.id = s.id
             where b.id = {id}
             """;

        var result = await ExecuteReaderListAsync(commandText, GetBasketLine, cancellationToken);

        return result.Count == 0
            ? null 
            : new Basket(result.First().Id, result.Select(i => i.Item)!, result.First().Customer);
    }
    
    private static BasketRow GetBasketLine(DbDataReader reader)
    {
        var basketId = reader.GetFieldValue<int>("basket_id");
        var customer = reader.GetFieldValue<string>("customer");
        var itemId = reader.GetFieldValue<int?>("item_id");
        if (itemId is null)
            return new BasketRow(basketId, customer, null);
        var itemType = (ItemTypes)reader.GetFieldValue<int>("type");
        SaleItem item = itemType == ItemTypes.Product
            ? new Product(reader.GetFieldValue<int>("item_id"),
                reader.GetFieldValue<string>("name"),
                reader.GetFieldValue<decimal>("price"),
                reader.GetFieldValue<int>("amount"))
            : new Service(reader.GetFieldValue<int>("item_id"),
                reader.GetFieldValue<string>("name"),
                reader.GetFieldValue<decimal>("price"));

        return new BasketRow(basketId, customer, new ItemsListLine(item, reader.GetFieldValue<int>("count")));
    }
}